# How to:
# New-WaykDenConfig -NatsUsername user -NatsPassword secret -RedisPassword secret123
# OR
# Set-WaykDenConfig -NatsUsername user -NatsPassword secret -RedisPassword secret123
# To show the info: Get-WaykDenConfig
# and then ./StartNatsRedis.ps1 -NatsUsername user -NatsPassword secret -RedisPassword secret123

param(
    [Parameter(Mandatory=$true, HelpMessage="Nats Username")]
    [string] $NatsUsername,
    [Parameter(Mandatory=$true, HelpMessage="Nats Password")]
    [string] $NatsPassword,
    [Parameter(Mandatory=$true, HelpMessage="Redis Password")]
    [string] $RedisPassword
)

function Start-Nats(
    [string] $NatsUsername,
    [string] $NatsPassword
){
    & docker stop den-nats
    & docker rm den-nats
    & docker run -d --network=den-network --name den-nats -p 4222:4222 nats --user $NatsUsername --pass $NatsPassword
}

function Start-Redis(
    [string] $RedisPassword
){
    & docker stop den-redis
    & docker rm den-redis
    & docker run -d --network=den-network -p 6379:6379 --name den-redis redis redis-server --requirepass $RedisPassword
}

function Create-Network{
    $network = $(docker network ls -qf “name=den-network”)
    if(!($network)){
        docker network create den-network
    }
}  

Create-Network
Start-Nats $NatsUsername $NatsPassword
Start-Redis $RedisPassword
