version: "3.6"

x-app: &default-app
  environment:
    - "READIUM_DATABASE_HOST=${READIUM_DATABASE_HOST}"
    - "READIUM_DATABASE_PORT=${READIUM_DATABASE_PORT}"
    - "READIUM_DATABASE_USERNAME=${READIUM_DATABASE_USERNAME}"
    - "READIUM_DATABASE_PASSWORD=${READIUM_DATABASE_PASSWORD}"
    - "READIUM_LCPSERVER_HOST=http://${READIUM_LCPSERVER_HOST}:${READIUM_LCPSERVER_PORT}"
    - "READIUM_LCPSERVER_PORT=${READIUM_LCPSERVER_PORT}"
    - "READIUM_LCPSERVER_DATABASE=${READIUM_LCPSERVER_DATABASE}"
    - "READIUM_LCPSERVER_USERNAME=${READIUM_LCPSERVER_USERNAME}"
    - "READIUM_LCPSERVER_PASSWORD=${READIUM_LCPSERVER_PASSWORD}"
    - "READIUM_LSDSERVER_HOST=http://${READIUM_LSDSERVER_HOST}:${READIUM_LSDSERVER_PORT}"
    - "READIUM_LSDSERVER_PORT=${READIUM_LSDSERVER_PORT}"
    - "READIUM_LSDSERVER_DATABASE=${READIUM_LSDSERVER_DATABASE}"
    - "READIUM_FRONTEND_HOST=http://${READIUM_FRONTEND_HOST}:${READIUM_FRONTEND_PORT}"
    - "READIUM_FRONTEND_PORT=${READIUM_FRONTEND_PORT}"
    - "READIUM_FRONTEND_DATABASE=${READIUM_FRONTEND_DATABASE}"
    - "READIUM_ENC_CONTENT=/opt/readium/files/encrypted"

services:
  database:
    build: ./database
    image: database
    ports:
      - "${READIUM_DATABASE_PORT}:3306"
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: "${READIUM_DATABASE_PASSWORD}"
    volumes:
      - "dbdata:/var/lib/mysql"

  sftp:
    image: "atmoz/sftp:alpine"
    ports:
      - "${READIUM_SFTP_PORT}:22"
    volumes:
      - "./files/users.conf:/etc/sftp/users.conf:ro"
      - "rawfiles:/home"

  lcpencrypt:
    <<: *default-app
    image: "readium/lcpencrypt:working"
    build:
      context: .
      target: "lcpencrypt"
    volumes:
      - "encfiles:/opt/readium/files/encrypted"
      - "rawfiles:/opt/readium/files/raw"

  lcpserver:
    <<: *default-app
    image: "readium/lcpserver:working"
    build:
      context: .
      target: "lcpserver"
    ports:
      - "${READIUM_LCPSERVER_PORT}:8989"
    volumes:
      - "encfiles:/opt/readium/files/encrypted"
      - "./etc:/etc/readium"
    depends_on:
      - database

  lsdserver:
    <<: *default-app
    image: "readium/lsdserver:working"
    build:
      context: .
      target: "lsdserver"
    ports:
      - "${READIUM_LSDSERVER_PORT}:8990"
    volumes:
      - "./etc:/etc/readium"
    depends_on:
      - database

  testfrontend:
    <<: *default-app
    image: "readium/testfrontend:working"
    build:
      context: .
      target: "testfrontend"
    ports:
      - "${READIUM_FRONTEND_PORT}:8991"
    volumes:
      - "encfiles:/opt/readium/files/encrypted"
      - "rawfiles:/opt/readium/files/raw"
      - "./etc:/etc/readium"
    depends_on:
      - database

volumes:
  encfiles:
  dbdata:
  rawfiles:
