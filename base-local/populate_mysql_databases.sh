#!/bin/bash

CONFIG_DB_HOST="192.168.1.248"
CONFIG_DB_ADMIN_PASSWORD="secretpassword"
CONFIG_DB_PORT="3326"

# If run immediately after docker-compose up -d, service may be finished loading
FINISHED=1
until [ $FINISHED -eq 0 ]
do
    echo "Waiting for MySQL service to load..."
    # Remove anonymous users as test
    mysql -h $CONFIG_DB_HOST -P $CONFIG_DB_PORT -u root -p$CONFIG_DB_ADMIN_PASSWORD -e "delete from mysql.user where user='';"
    FINISHED=$?
    if [ $FINISHED -gt 0 ]; then
        # The  MySQL command failed, so the server is not ready yet; 
        # wait 5 seconds before trying again
        sleep 5
    fi
done

echo ""
echo "Loading MySQL script to create databases and users..."
echo ""

# Create databases, users, and assign privileges using SQL script
mysql -h $CONFIG_DB_HOST -P $CONFIG_DB_PORT -u root -p$CONFIG_DB_ADMIN_PASSWORD < create_db_and_users.sql

echo ""
echo "Downloading LCP data schemas from project repository..."
echo ""

# Download MySQL schemas provided by Readium
# https://github.com/readium/readium-lcp-server/tree/master/dbmodel
wget https://raw.githubusercontent.com/readium/readium-lcp-server/master/dbmodel/mysql_db_setup_lcpserver.sql
wget https://raw.githubusercontent.com/readium/readium-lcp-server/master/dbmodel/mysql_db_setup_lsdserver.sql
wget https://raw.githubusercontent.com/readium/readium-lcp-server/master/dbmodel/mysql_db_setup_frontend.sql

# Note that, if using Amazon Aurora Serverless, you need to change the
# VARCHAR(255) columns to VARCHAR(191) for compatibility with
# default-settings Aurora:
#sed -i 's/varchar(255)/varchar(191)/g' mysql_db_setup_lsdserver.sql
#...

echo ""
echo "Loading data..."
echo ""

# Now populate the databases with tables
mysql -h $CONFIG_DB_HOST -P $CONFIG_DB_PORT -u root -p$CONFIG_DB_ADMIN_PASSWORD lcpserver < mysql_db_setup_lcpserver.sql
mysql -h $CONFIG_DB_HOST -P $CONFIG_DB_PORT -u root -p$CONFIG_DB_ADMIN_PASSWORD lsdserver < mysql_db_setup_lsdserver.sql
mysql -h $CONFIG_DB_HOST -P $CONFIG_DB_PORT -u root -p$CONFIG_DB_ADMIN_PASSWORD testfrontend < mysql_db_setup_frontend.sql

echo ""
echo "Removing schema scripts..."
echo ""

# Remove the initial schema scripts
rm mysql_db_setup_lcpserver.sql
rm mysql_db_setup_lsdserver.sql
rm mysql_db_setup_frontend.sql

echo ""
echo "Populating databases complete."
