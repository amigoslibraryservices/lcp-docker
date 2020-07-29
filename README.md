# Docker-Based Implementation of Readium LCP #
*Version 0.50*

[Readium LCP](https://www.edrlab.org/readium-lcp/) (Licensed Content Protection) is an encryption technology for protecting ebook content published in epub and pdf formats, produced by the [European Digital Reading Lab](https://www.edrlab.org/about/) (EDRLab). It is a form of digital rights management (DRM) software, an open source alternative to proprietary solutions such as the Adobe Content Server (ACS) product. A portion of LCP requires a proprietary certificate (to ensure security), but the [codebase for LCP](https://github.com/readium/readium-lcp-server) is made available under a BSD 3-clause license. Other than fees for maintaining the efficacy of the certificate system, there are no commercial fees for the use of LCP in the ebook publishing-to-consumption toolchain.

This project is a demonstration deployment system for making ebooks available using LCP technology. There is support for two deployment strategies in this initial version of the project:
* **base-local**: simple `docker-compose.yaml` and `Dockerfile` file for deploying all services on a local Docker-based computer;
* **ansible-single**: a simple set of Ansible files for deploying on a local Docker host. *In this initial release, the Ansible files are for review only. These are not quite complete and will be expanded in future iterations. In addition, they will include deployment in a single-host stack in either a VPS-based instance such as Linode.com or an ECS cluster in AWS.*

*This project is funded in part by the U.S. Institute of Museum and Library Services through a grant to the Texas State Library and Archives Commission, Grant Number COP-20001 (2020). It is licensed under the [BDS 3-clause license](https://choosealicense.com/licenses/bsd-3-clause/).*

This document is a work in progress. Several *TODOs* mark additional features we are working on.

## Components of the Project ##
As a means to create a simple local LCP service deployment which also mimics what a production system might look like, we created each LCP service as a Docker-built image. In addition, we have added other Docker containers to demonstrate a complete system.

* **database**: a standard MariaDB/MySQL service using LCP's basic MySQL schema
* **sftp**: a Secure File Transfer Protocol (SFTP) service enabling outside upload of unencrypted (*raw*) ebooks in batch submission
* **nginx-proxy (TODO)**: its primary purpose in the demonstration is to provide an access endpoint for the publicly available collection of encrypted works managed by the LCP system; in a more robust system, may provide reverse-proxy access to LCP or other resources
* **lcpencrypt**: the demonstration tool provided by EDRLab to encrypt raw epub or pdf ebooks for distribution by the LCP system
* **lcpserver**: the License Server which creates licenses to encrypted works
* **lsdserver**: the License Status Document server which intermediates end-user communication to the License Server related to license updates and status requests
* **testfrontend**: the demonstration "front end" service provided by EDRLab for simple testing, enabling public users to browse a list of available ebooks and request licenses and acquire them (*purchases* in this demo, whether limited term (loan) or permanent); this corresponds to a production provider's e-commerce or catalog system 

### Encrypted File Store ###
In order to simulate a distributed production service, we have chosen to implement the encrypted file store (analogous to the Adobe Content Server media server) initially as a shared Docker resource (a Docker volume). In future iterations, we will implement an AWS S3 bucket (complete with secure write access for initial storage) and also an AWS Elastic File System (EFS) file system.

### Master File Store ###
This file store is the initial "holding" location for uploaded ebooks prior to encryption, ingest into the License Server database, and upload to the encrypted file store. It is also implemented as a shared Docker volume and will be enhanced as an AWS EFS file system.

Ultimately, in a production system, *raw* ebooks stored here would be moved, once encrypted, into a secure vault in another location.

### SFTP Users ###
To facilitate a demonstration of uploading, we have added a file of user data (`users.conf`). The file includes usernames and plain passwords. The user data will be transferred into the sftp container when it is instantiated. A process in the container will then add these users to the SFTP server. Feel free to customize or add user data as desired.

### Uploaded File Watching ###
If the SFTP service is used, the question arises regarding how the system will know new ebooks have arrived for encryption and addition to the License Server. To facilitate a quick-n-dirty demonstration, we have added the Python watchdog utility to watch for epub and pdfs deposited in the users' uploads directory (actually, the downloads directory as well). *TODO: This process will be enhanced in upcoming iterations so that file naming and storage can be customized. Also, at least basic title data and the encrypted work's content-id need to be added to the front end so they appear in the Publications list to a testing user.*

## Deploying using Docker Compose ##

1. Clone this repository.
2. Change directories into the `base-local` directory.
3. Adjust if needed values in [.env file](./env)
4. Determine whether to store encrypted files on the local file system or in AWS S3 (*hint: for this initial version, choose filesystem*).
5. Copy the proper files based on the storage selection; for example:
    ```bash
    cp docker-compose.yaml.fs docker-compose.yaml
    cp etc/config.yaml.fs etc/config.yaml
    ```
6. When ready, run the Docker Compose command in the base-local directory to instantiate the Docker stack:
    ```bash
    sudo docker-compose up -d
    ```
7. Wait till the database starts and is ready to connect to using the following command:
    ```bash
    docker-compose logs -f database
    ```
8. After the database is fully up and ready restart `lcpserver`, `lsdserver`, and `testfrontend`:
    ```bash
    sudo docker-compose up -d
    ```
Once the services are started, the system should be active.

### To Access a Command Line ###
If you happen to need to access a command inside one of the LCP containers, issue the following Docker command (for example):
```bash
docker-compose exec testfrontend /bin/ash
```
Note: The containers are built using the alpine base image, which uses the Ash shell instead of Bash.


## A Simple Test ##

### Ensuring the Containers Start Successfully ###
To be sure the containers are started successfully (typically, the sftp and database containers start without problems), run the following command. Note that the containers may be named with a prefix (the directory in which the docker-compose.yaml file is located) and a suffix (typically a '1').
```bash
sudo docker ps -a
```
If any of the LCP containers have *Exited*, then execute a command to list the logs for the failed container:
```bash
docker-compose logs lcpserver
```
This should present an error or condition indicating why the container exited. If all of the containers are running, you should be good to proceed.

### Upload a Book via the Front-end ###
For local testing you'll need to adjust your [hosts file](/etc/hosts) and add the following string there:
```
127.0.0.1 testfrontend
```
It's required because `testfrontend` host name is used in the front-end source code.

After that access the front-end in a web browser using the following address:
```bash
http://localhost:8991
```
NOTE: the port might be different, please refer to [.env file](./base-local/.env) for the correct value

You will receive a notice that the front-end app (a simple Node.js app) is loading. Once the interface is displayed, upload a book and add a user.
1. Click the *Publications* menu item
2. Click *Add a Publication*
3. The easiest way to upload the file is to open a window, navigate to a folder with a raw epub or pdf file in it, and drag-and-drop the file onto the large rectangle in the middle of the form.
4. Enter a title for the book
5. Click the *Save* button
6. The page will refresh and display a list of publications available in the system.
7. Click the *Users* menu item
8. Click *Add a User*
9. Enter a username
10. Enter an email address
11. Enter a passphrase; passphrases can be longer than passwords, but their function is the same in the system. In order to open encrypted ebooks from the system when you acquire them (purchase in this interface), you will need to supply your passphrase in your reading application.
12. Click the *Save* button

*TODO: Revise this into a series of docs.*

### Download and Read an LCP-Encrypted Ebook ###

*TODO: Future iterations to add an OPDS feed to present a catalog for use in Thorium Reader or other LCP-compliant reader.*
