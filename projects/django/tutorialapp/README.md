To create the django project

`sudo docker-compose run web django-admin startproject <name-of-project> .`

Change ownership of files created

`sudo chown -R $USER:$USER .`

To run the server

`docker-compose up`

`docker container ls`

Can remove django project directory at shutdown