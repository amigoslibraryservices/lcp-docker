version: '3.6'
services:

    database:
        image: "mariadb:latest"
        ports:
          - "3326:3306"
        restart: always
        environment:
            MYSQL_ROOT_PASSWORD: "secretpassword"
        volumes:
          - "dbdata:/var/lib/mysql"

    sftp:
        image: "atmoz/sftp:alpine"
        ports:
          - "8922:22"
        volumes:
          - "./files/users.conf:/etc/sftp/users.conf:ro"
          - "rawfiles:/home"

    lcpencrypt:
        image: "readium/lcpencrypt:working"
        build:
            context: .
            target: "lcpencrypt"
        environment:
          - "READIUM_LCPSERVER_HOST=http://192.168.1.253:8989"
          - "READIUM_LCPSERVER_USERNAME=lcp"
          - "READIUM_LCPSERVER_PASSWORD=secretpassword"
        volumes:
          - "encfiles:/opt/readium/files/encrypted"
          - "rawfiles:/opt/readium/files/raw"

    lcpserver:
        image: "readium/lcpserver:working"
        build:
            context: .
            target: "lcpserver"
        environment:
          - "READIUM_DATABASE_HOST=192.168.1.253"
          - "READIUM_DATABASE_PORT=3326"
          - "READIUM_DATABASE_USERNAME=root"
          - "READIUM_DATABASE_PASSWORD=secretpassword"
          - "READIUM_DATABASE_DATABASE=lcpserver"
          - "READIUM_LCPSERVER_HOST=http://192.168.1.253:8989"
          - "READIUM_LSDSERVER_HOST=http://192.168.1.253:8990"
          - "READIUM_FRONTEND_HOST=http://192.168.1.253:8991"
          - "READIUM_ENC_CONTENT=/opt/readium/files/encrypted"
        ports:
          - 8989:8989
        volumes:
          - "encfiles:/opt/readium/files/encrypted"
        depends_on:
          - database

    lsdserver:
        image: "readium/lsdserver:working"
        build:
            context: .
            target: "lsdserver"
        environment:
          - "READIUM_DATABASE_HOST=192.168.1.253"
          - "READIUM_DATABASE_PORT=3326"
          - "READIUM_DATABASE_USERNAME=root"
          - "READIUM_DATABASE_PASSWORD=secretpassword"
          - "READIUM_DATABASE_DATABASE=lsdserver"
          - "READIUM_LCPSERVER_HOST=http://192.168.1.253:8989"
          - "READIUM_LSDSERVER_HOST=http://192.168.1.253:8990"
          - "READIUM_FRONTEND_HOST=http://192.168.1.253:8991"
        ports:
          - 8990:8990
        depends_on:
          - database


    testfrontend:
        image: "readium/testfrontend:working"
        build:
            context: .
            target: "testfrontend"
        environment:
          - "READIUM_DATABASE_HOST=192.168.1.253"
          - "READIUM_DATABASE_PORT=3326"
          - "READIUM_DATABASE_USERNAME=root"
          - "READIUM_DATABASE_PASSWORD=secretpassword"
          - "READIUM_DATABASE_DATABASE=testfrontend"
          - "READIUM_LCPSERVER_HOST=http://192.168.1.253:8989"
          - "READIUM_LSDSERVER_HOST=http://192.168.1.253:8990"
          - "READIUM_FRONTEND_HOST=http://192.168.1.253:8991"
          - "READIUM_ENC_CONTENT=/opt/readium/files/encrypted"
        ports:
          - 8991:8991
        volumes:
          - "encfiles:/opt/readium/files/encrypted"
          - "rawfiles:/opt/readium/files/raw"
        depends_on:
          - database

volumes:
    encfiles:
    dbdata:
    rawfiles:
