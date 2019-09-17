#!/usr/bin/env python3

# Requires pyunpack and patool to be installed

import getopt, sys
import os.path
import re
import json
import shutil
from pyunpack import Archive
import patoolib

from contextlib import contextmanager
import pip

required_pkgs = ['pyunpack', 'patool']

@contextmanager
def suppress_stdout():
    with open(os.devnull, "w") as devnull:
        old_stdout = sys.stdout
        sys.stdout = devnull
        try:  
            yield
        finally:
            sys.stdout = old_stdout

def CheckAndInstallDependencies():
	installed_pkgs = [pkg.key for pkg in pip.get_installed_distributions()]
	
	for package in required_pkgs:
		if package not in installed_pkgs:
			print("%s not installed, attempting to install with pip..." % package)
			with suppress_stdout():
				pip.main(['install', package])

def usage():
	strUsage = """
	This will unpack a directory as given by MyCourses into a set of 
	directories by student name containing all files provided, and those
	respectively unpacked into folders

	Dependencies: 
	sudo apt-get install p7zip-full
	pip3 install patool
	pip3 install pyunpack
	"""
	print(strUsage)
	sys.exit(0)

def CheckDirectory(strDirectoryPath):
	if(os.path.exists(strDirectoryPath)):
		return True
	else:
		return False

def CheckFileExists(strFilePath):
	if(os.path.exists(strFilePath) and os.path.isfile(strFilePath)):
		return True
	else:
		return False
	
CheckAndInstallDependencies()

fullCmdArguments = sys.argv

argumentList = fullCmdArguments[1:]

unixOptions = "hd:n:a:x"
gnuOptions = ["help", "directory=", "name=", "archive="]

try:
	arguments, values = getopt.getopt(argumentList, unixOptions, gnuOptions)
except getopt.error as err:
	# Output error
	print(str(err))
	sys.exit(2)

strGradingDirectoryPath = ""
strName = ""
strArchiveName = ""
fDeleteArchive = False

# Evaluate the options
for currentArgument, currentValue in arguments:
	if currentArgument in ("-h", "--help"):
		usage()
	elif currentArgument in ("-d", "--directory"):
		strGradingDirectoryPath = currentValue
	elif currentArgument in ("-n", "--name"):
		strName = currentValue
	elif currentArgument in ("-x"):
		strName = currentValue
	elif currentArgument in ("-a", "--archive"):
		strArchiveName = currentValue

if(not strGradingDirectoryPath):
	print("Directory path not provided\n")
	usage()

if(CheckDirectory(strGradingDirectoryPath) == False):
	print("%s is not a valid directory on file system" % strGradingDirectoryPath)
	usage()

# if not provided, will just use name
if(strArchiveName):
	# find in dir
	strFileName = ""
	root, dirs, files = next(os.walk(strGradingDirectoryPath))
	for fileName in files:
		if(fileName.find(strArchiveName) != -1):
			strFileName = fileName
			break

	print("Archive %s found so lets unarchive it\n" % strFileName)
	strArchiveFilePath = os.path.join(strGradingDirectoryPath, strFileName)
	if(CheckFileExists(strArchiveFilePath) == False):
		print("Error: %s not found" % strArchiveFilePath)
		sys.exit(2)

	strArchiveFolderName = os.path.splitext(strFileName)[0]
	strArchiveFolderName = os.path.join(strGradingDirectoryPath, strArchiveFolderName)
	print("Creating folder %s" % strArchiveFolderName)
	dirMode = 0o666
	os.mkdir(strArchiveFolderName, dirMode)

	print("Extracting %s to\n %s" % (strArchiveFilePath, strArchiveFolderName))
	Archive(strArchiveFilePath).extractall(strArchiveFolderName)
	
	if(fDeleteArchive == True):
		os.remove(strArchiveFilePath)
		print("Extrated and deleted %s " % strArchiveFilePath)
	else:
		print("Extrated %s " % strArchiveFilePath)

	strName = strArchiveName

if(not strName):
	print("Exercise name folder string not provided\n")
	usage()

strDirName = ""
# Validated the directory, find the exercise directory 
root, dirs, files = next(os.walk(strGradingDirectoryPath))
for dirName in dirs:
	if(dirName.find(strName) != -1):
		strDirName=dirName
		break

if(not strDirName):
	print("Exercise name folder with identifier %s not found" % strName)
	sys.exit(2)

strNamedPath = strGradingDirectoryPath + strDirName
if(CheckDirectory(strNamedPath) == False):
	print("%s could not be found" % strNamedPath)
	sys.exit(2)

students = {}
print("Found and entering %s - files:" % strNamedPath)
root, dirs, files = next(os.walk(strNamedPath))
for fileName in files:
	#print(fileName)
	regexProgram = re.compile(r'\d+-\d+ - ([\w-]+, \w+)')
	result = regexProgram.match(fileName)
	if result:
		#print(result.group(1))
		strStudentName = result.group(1)
		if(strStudentName not in students):
			students[strStudentName] = []
		students[strStudentName].append(fileName)

#print(json.dumps(students, indent=4, sort_keys=True))

# Set up directory structure
for studentName in students:
	files = students[studentName]
	#print(student)
	#print(files)
	# make a directory for each student
	studentPath = studentName.lower().replace(",", "_").replace(" ", "")
	studentPath = os.path.join(strNamedPath, studentPath)
	
	if(CheckDirectory(studentPath) == False):
		print("Created folder for %s" % studentPath)
		dirMode = 0o666
		os.mkdir(studentPath, dirMode)
	else:
		print("Directory exists for %s" % studentPath)

	for fileName in files:
		srcFilePath =  os.path.join(strNamedPath, fileName)
		
		if(CheckFileExists(srcFilePath) == False):
			print("Error: %s not found" % srcFilePath)
			sys.exit(2)

		destFilePath = os.path.join(studentPath, fileName)
		if(CheckFileExists(destFilePath) == True):
			print("Warning: %s already exists, skipping" % destFilePath)
			sys.exit(2)

		# Convert to file name
		regexProgram = re.compile(r"(?!.+ - )- ((.*)\.(\S{0,4}$))")
		result = regexProgram.search(fileName)

		if result:
			strFileName = result.group(1)
			strFileStripped = result.group(2)
			strFileExtension = result.group(3)
		else:
			print("ERROR: %s failed the regex" % fileName)
			sys.exit(2)

		# This will move the file
		print("Moving %s to %s folder" % (fileName, studentName.lower().replace(",", "_").replace(" ", "")))
		shutil.move(srcFilePath, destFilePath)

		# is it an archive
		if (strFileExtension.lower() == 'zip' or strFileExtension.lower() == '7z'):
			strFileArchivePath = os.path.join(studentPath, strFileStripped)

			i = 1
			while(CheckDirectory(strFileArchivePath) == True):
				print("Warning: %s already exists using full path" % strFileArchivePath)
				strFileArchivePath = os.path.join(studentPath, strFileStripped + str(i))
				i += 1
			
			print("Creating folder %s" % strFileArchivePath)
			dirMode = 0o666
			os.mkdir(strFileArchivePath, dirMode)

			print("Extracting %s to\n %s" % (strFileName, strFileArchivePath))
			Archive(destFilePath).extractall(strFileArchivePath)
			
			if(fDeleteArchive == True):
				os.remove(destFilePath)
				print("Extrated %s and deleted" % strFileName)
			else:
				print("Extrated %s" % strFileName)

# Done
print("Done!")