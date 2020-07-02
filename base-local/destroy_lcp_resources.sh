#!/bin/bash

echo "Shutting down and removing Docker containers..."

# Stop and remove Docker containers; also removes network
docker-compose down

echo "Removing Docker volumes..."

# Get current directory name, which is a prefix of the Docker volume names
DIR_PATH=$(dirname "$(readlink -f "$0")")
PREFIX=$(basename $DIR_PATH)
DBDATA=$PREFIX"_dbdata"
ENCFILES=$PREFIX"_encfiles"
RAWFILES=$PREFIX"_rawfiles"
# Remove volumes
docker volume rm $DBDATA
docker volume rm $ENCFILES
docker volume rm $RAWFILES

echo "Deleting downloaded or built Docker images..."

# Remove images
docker rmi readium/testfrontend:working
docker rmi readium/lsdserver:working
docker rmi readium/lcpserver:working
docker rmi readium/lcpencrypt:working
docker rmi atmoz/sftp:alpine
docker rmi mariadb:latest

echo "Resources removed successfully."
