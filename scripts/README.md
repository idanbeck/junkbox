# Junkbox Scripts

## fastai-instance.sh

Finally spurred on by the https://www.fast.ai/ course and general deep learning, this script allows for easier manipulation of AWS instances. Below is the usage doc:

### Usage
fastai-instance <command> -i <instanceID> -u <username> -p <pem file>

### Commands
start - Starts the given instance ID
stop - Stops the given instance ID
connect - Will connect over SSH to the instance ID and map port 8888 to localhost
-x will stop the instance on ssh exit
-s will start the instance on connect, and poll state until ready to connect then will attempt to connect

### Examples

./fastai-instance.sh connect -i <instance id> -p <pem file> -x -s

This will connect to the instance ID only after it has been started and is running.  This will also test to ensure that the instance is not stopping, and will wait until the instance is stopped before running.  The -x modifier will also stop the instance as soon as you exit the ssh session (although I don't think it will do this if you exit the terminal). 

### Requirements 

Needs to have aws command line tools and jq installed
