#!/bin/sh
HOSTNAME="binaryvibrance.net"

docker stop slate-message-bus
docker stop slate-mongo-character
docker stop slate-mongo-auth
docker stop slate-mongo-graylog
docker stop slate-elasticsearch
docker stop slate-graylog

# RABBIT MQ
# With Management
docker run -d --hostname slate-message-bus.$HOSTNAME --rm --name slate-message-bus -p 15672:15672 -p 5672:5672 rabbitmq:3.8-management
# Without Management
#docker run -d --hostname slate-message-bus.$HOSTNAME --name slate-message-bus -p 5672:5672 rabbitmq:3.8

# MONGODB
# Auth database
docker run -d --hostname slate-auth-db.$HOSTNAME --rm --name slate-mongo-auth -p 27017:27017 mongo:5.0-rc
# Character databse
docker run -d --hostname slate-character-db.$HOSTNAME --rm --name slate-mongo-character -p 27018:27017 mongo:5.0-rc

# GRAYLOG
docker run -d --rm --name slate-mongo-graylog mongo:4.2
docker run -d --rm --name slate-elasticsearch \
           --hostname slate-elasticsearch \
           -e "http.host=0.0.0.0" \
           -e "discovery.type=single-node" \
           -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
           docker.elastic.co/elasticsearch/elasticsearch-oss:7.10.2
docker run -d --rm --name slate-graylog \
           --link slate-mongo-graylog \
           --link slate-elasticsearch \
           -p 9000:9000 -p 12201:12201 -p 1514:1514 \
           -e GRAYLOG_HTTP_EXTERNAL_URI="http://127.0.0.1:9000/" \
           -e GRAYLOG_MONGODB_URI="mongodb://slate-mongo-graylog/graylog" \
           -e GRAYLOG_ELASTICSEARCH_HOSTS="http://slate-elasticsearch:9200" \
           graylog/graylog:4.0

docker ps