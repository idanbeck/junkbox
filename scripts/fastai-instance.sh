#!/bin/bash

usage()
{
	echo "fastai-instance <command> -i <instanceID> -u <username> -p <pem file>"
	echo "Commands"
	echo "start - Starts the given instance ID"
	echo "stop - Stops the given instance ID"
	echo "connect - Will connect over SSH to the instance ID and map port 8888 to localhost"
	echo "-x will stop the instance on ssh exit"
	echo "-s will start the instance on connect, and poll state until ready to connect then will attempt to connect"
	exit
}

if ! [ -x "$(command -v jq)" ]
then
	echo "jq not installed - please install before using"
	exit
fi

if ! [ -x "$(command -v aws)" ]
then
	echo "aws command line tools not installed - please install before using"
	exit
fi

strInstanceID=""
strInstancePEMFile=""
strUsername="ubuntu"
stopInstanceOnExitSHH=0
startInstanceOnConnectSSH=0

StartInstance()
{
	echo "Starting Instance $strInstanceID"
	aws ec2 start-instances --instance-ids $strInstanceID 
}

StopInstance()
{
	echo "Stopping Instance $strInstanceID"
	aws ec2 stop-instances --instance-ids $strInstanceID
}

instanceState=""

UpdateInstanceState()
{
	# First see if instance is running
	instanceState=$(aws ec2 describe-instances | jq ".Reservations[].Instances[] | {id: .InstanceId, state:.State.Name} | select(.id == \"$strInstanceID\") | .state")
	temp="${instanceState%\"}"
	instanceState="${temp#\"}"

}

ConnectToInstance() 
{
	UpdateInstanceState

	if [ $startInstanceOnConnectSSH != 0 ]
	then
		echo "Start instance on connect selected, will attempt to start the instance"
		
		# Need to check if it's stopping (since we'll get an error when we
		# try to start again
		if [ $instanceState == "stopping" ] 
		then
			while [ $instanceState == "stopping" ]; do
				echo "Instance is stopping, waiting 10s more until stopped"
				sleep 10
				UpdateInstanceState
			done
			echo "Instance stopped"
		fi

		if [ $instanceState != "running" ]
		then
			echo "Starting instance"
			StartInstance
			
			echo "Wait until instance is running"
			while [ $instanceState != "running" ]; do
				echo "Instance still not running, wait another 10 seconds"
				UpdateInstanceState
				sleep 10
			done
		fi		
	fi		

		
	if [ "$instanceState" != "running" ] 
	then
		echo "Instance $strInstanceID is $instanceState - please make sure it is running"
		usage
		exit
	fi
	
	echo "Connecting to instance $strInstanceID"
	instanceDNS=$(aws ec2 describe-instances | jq ".Reservations[].Instances[] | {id: .InstanceId, state:.State.Name, dns: .PublicDnsName} | select(.state == \"running\") | select(.id == \"$strInstanceID\") | .dns")
	temp="${instanceDNS%\"}"
	instanceDNS="${temp#\"}"

	sshCommand="ssh -i \"$strInstancePEMFile\" $strUsername@$instanceDNS -L localhost:8888:localhost:8888" 
	echo $sshCommand
	eval $sshCommand

	# This will stop instance on exit
	if [ $stopInstanceOnExitSSH != 0 ]
	then
		echo "Stopping instance"
		StopInstance
		exit
	fi
}

strCommand=$1
shift

while [ "$1" != "" ]; do
	case $1 in
		-i | --instance )	shift
							strInstanceID=$1
							;;
		
		-u | --username )	shift
							strUsername=$1
							;;

		-h | --help )		shift
							usage
							exit
							;;
		
		-p | --pem )		shift
							strInstancePEMFile=$1
							;;

		# Modifiers
		-x | --stop-on-exit )	stopInstanceOnExitSSH=1
								;;
		
		-s | --start-on-connect )	echo "start on connect"
									startInstanceOnConnectSSH=1
									;;
		
		* )					shift
							usage
							exit
							;;
	esac
	shift
done

if [ "$strInstanceID" == "" ]
then
	echo "no instance ID set"
	usage
	exit
fi

case $strCommand in
	start )		StartInstance
				exit
				;;

	stop )		StopInstance
				exit	
				;;

	connect )	ConnectToInstance
				;;

	* )			usage
				;;
esac


