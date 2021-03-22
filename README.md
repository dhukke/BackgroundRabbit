# BackgroundRabbit

Publisher and Subscriber Backgroundservices with RabbitMQ. 

# To test using docker

`
docker run -d --hostname rabbitserver --name rabbitmq-server -p 15672:15672 -p 5672:5672 rabbitmq:3-management
`


`
docker run --name sqlserver -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=yourStrong(!)Password" -p 1433:1433 -d mcr.microsoft.com/mssql/server
`
