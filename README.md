# dotnet-elasticsearch-sample
ElasticSearch sample with .NET

Run ElasticSearch container with podman.
```shell
podman run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" -e "xpack.security.enabled=false" docker.elastic.co/elasticsearch/elasticsearch:8.13.2
```