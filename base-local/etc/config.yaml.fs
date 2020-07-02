# Configuration Settings for LCP servers
# Shared configuration, all services in one file
# Encrypted Resource type: 
#   * For initial testing, filesystem only

# The usernames and passwords must match the ones in the htpasswd files
# for each server.

lcp_update_auth:
    username: "lcp"
    password: "secretpassword"

lsd_notify_auth:
    username: "lcp"
    password: "secretpassword"


# LCP Server

profile: "basic"
lcp:
    host: "0.0.0.0"
    port: 8989
    public_base_url: "$READIUM_LCPSERVER_HOST"
    database: "mysql://$READIUM_DATABASE_USERNAME:$READIUM_DATABASE_PASSWORD@tcp($READIUM_DATABASE_HOST:$READIUM_DATABASE_PORT)/$READIUM_DATABASE_DATABASE?parseTime=true"
    auth_file: "/etc/readium/htpasswd"
storage:
    mode: "filesystem"
    filesystem:
        directory: "$READIUM_ENC_CONTENT"
certificate:
    cert: "/etc/readium/certificate.pub"
    private_key: "/etc/readium/certificate.pem"
license:
    links:
        status: "$READIUM_LSDSERVER_HOST/licenses/{license_id}/status"
        hint: "$READIUM_FRONTEND_HOST/static/hint.html"
        publication: "$READIUM_LCPSERVER_HOST/contents/{publication_id}"


# LSD Server

lsd:
    host: "0.0.0.0"
    port: 8990
    public_base_url: "$READIUM_LSDSERVER_HOST"
    database: "mysql://$READIUM_DATABASE_USERNAME:$READIUM_DATABASE_PASSWORD@tcp($READIUM_DATABASE_HOST:$READIUM_DATABASE_PORT)/$READIUM_DATABASE_DATABASE?parseTime=true"
    auth_file: "/etc/readium/htpasswd"
    license_link_url: "$READIUM_FRONTEND_HOST/api/v1/licenses/{license_id}"
license_status:
    register: true
    renew: true
    return: true
    renting_days: 60
    renew_days: 7


# Frontend Server

frontend:
    host: "0.0.0.0"
    port: 8991
    public_base_url: "$READIUM_FRONTEND_HOST"
    database: "mysql://$READIUM_DATABASE_USERNAME:$READIUM_DATABASE_PASSWORD@tcp($READIUM_DATABASE_HOST:$READIUM_DATABASE_PORT)/$READIUM_DATABASE_DATABASE?parseTime=true"
    master_repository: "/opt/readium/files/raw/frontend/uploads"
    encrypted_repository: "$READIUM_ENC_CONTENT"
    provider_uri: "https://www.myprovider.org"
    right_print: 10
    right_copy: 2000
