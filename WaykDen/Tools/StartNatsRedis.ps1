# How to:
# New-WaykDenConfig -NatsUsername david -NatsPassword 123456 -RedisPassword 123456789
# OR
# Set-WaykDenConfig -NatsUsername david -NatsPassword 123456 -RedisPassword 123456789
# To show the info: Get-WaykDenConfig
# en then ./StartNatsRedis.ps1 -NatsUsername david -NatsPassword 123456 -RedisPassword 123456789

param(
    [Parameter(Mandatory=$false, HelpMessage="Nats Username")]
    [string] $NatsUsername,
    [Parameter(Mandatory=$false, HelpMessage="Nats Password")]
    [string] $NatsPassword,
    [Parameter(Mandatory=$false, HelpMessage="Redis Password")]
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

Start-Nats $NatsUsername $NatsPassword
Start-Redis $RedisPassword