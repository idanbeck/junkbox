#!/bin/bash

docker run -p 8888:8888 --volume=$(pwd)/notebooks:/notebooks notebook_docker
