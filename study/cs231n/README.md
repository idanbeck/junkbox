
## Connecting to instance 

'gcloud compute --project "<YOUR_PROJECT_ID>" ssh --zone "us-west1-b" "<YOUR_VM_NAME>"'

## Transferring files from instance to computer

'gcloud compute scp <user>@<instance-name>:/path/to/file.zip /local/path'

Full folder
'gcloud compute scp --recursive /my/local/folder tonystark@cs231n:/home/shared/'

Also useful is rsync to synchronize files/folders between local machine and remote server
