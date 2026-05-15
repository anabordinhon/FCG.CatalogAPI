# FCG.Catalog.API

## Descricao

Microservico responsavel pelo catalogo de jogos da plataforma, incluindo cadastro e consulta de jogos, promocoes, compras e busca textual.

A aplicacao roda em .NET 8 e hoje integra os seguintes componentes de infraestrutura:

- SQL Server para persistencia transacional.
- Redis para cache distribuido.
- Elasticsearch para indexacao e busca de jogos.
- RabbitMQ com MassTransit para mensageria e consumo de eventos.
- Quartz para rotinas agendadas.
- OpenTelemetry e AWS CloudWatch para observabilidade.

## Visao geral tecnica

- API HTTP exposta na porta `8080`.
- Swagger habilitado na aplicacao.
- Health check em `/health`.
- Autenticacao via JWT Bearer.
- Indexacao automatica no Elasticsearch durante a inicializacao.
- Cache distribuido para detalhes de jogo e ranking de mais vendidos.
- Job agendado para refresh do cache de top sellers.
- Manifests Kubernetes em `FCG.Catalog/k8s/`.

## Estrutura relevante

- `FCG.Catalog/FCG.Catalog.API`: camada de entrada, configuracao e controllers.
- `FCG.Catalog/FCG.Catalog.Application`: casos de uso e contratos.
- `FCG.Catalog/FCG.Catalog.Domain`: entidades e regras de dominio.
- `FCG.Catalog/FCG.Catalog.Infrastructure`: persistencia, cache, Elastic e mensageria.
- `FCG.Catalog/k8s`: manifests para deploy no Kubernetes.

## Dependencias de infraestrutura

### SQL Server

A conexao principal da aplicacao usa `ConnectionStrings__DefaultConnection`.

Responsabilidades:

- Persistencia de jogos, promocoes e compras.
- Execucao de migrations via EF Core.
- Tabelas temporais nas entidades que ja possuem esse suporte nas migrations atuais.

Exemplo de connection string:

```text
Server=sqlserver-service,1433;Database=FCGCatalog;User Id=sa;Password=<SENHA>;TrustServerCertificate=True;Encrypt=False;
```

### Redis

O Redis e usado como cache distribuido pela API.

Configuracoes suportadas:

- `Redis__ConnectionString`
- `Redis__InstanceName`
- `Redis__GameCacheTtlMinutes`
- `Redis__TopSellingCacheTtlMinutes`

Comportamento atual:

- Prefixo padrao de chave: `fcg:catalog:`.
- TTL padrao do cache de jogo: `60` minutos.
- TTL padrao do cache de top sellers: `120` minutos.

### Elasticsearch

O Elasticsearch e usado para busca textual de jogos.

Configuracao suportada:

- `Elasticsearch__Uri`

Comportamento atual:

- URI padrao no codigo: `http://fcg-elasticsearch:9200` quando a configuracao nao e informada.
- Indice utilizado: `fcg-games`.
- O indice e criado automaticamente no startup, se nao existir.
- O analisador configurado aplica `lowercase` e `asciifolding`, o que melhora buscas sem sensibilidade a maiusculas e acentos.

### RabbitMQ e MassTransit

A mensageria da aplicacao usa RabbitMQ com MassTransit.

Configuracoes suportadas:

- `RabbitMQ__Host`
- `RabbitMQ__Username`
- `RabbitMQ__Password`

Comportamento atual:

- Consome o evento `PaymentProcessedEvent`.
- Publica e consome mensagens com nomes de entidade como `payment-processed` e `order-placed`.
- Fila de consumo configurada: `payment-processed-catalog-queue`.
- Se usuario e senha nao forem informados, o codigo usa `guest` e `guest`.

### Quartz

Quartz e usado para processamento agendado.

Configuracao suportada:

- `Quartz__TopSellingGamesCron`

Comportamento atual:

- Valor padrao: `0 30 * ? * *`.
- Job configurado: `RefreshTopSellingGamesJob`.
- Objetivo: atualizar o cache de jogos mais vendidos.

### Observabilidade

A aplicacao possui telemetria e logging com OpenTelemetry e integracao com AWS.

Configuracoes suportadas:

- `OpenTelemetry__CollectorEndpoint`
- `AWS__Region`
- `AWS__Logging__LogGroup`

Comportamento atual:

- Exportacao OTLP via gRPC.
- Endpoint padrao do collector: `http://host.docker.internal:4317`.
- Log group padrao: `/fcg/catalog/api`.
- Regiao AWS padrao: `us-east-1`.

## Variaveis de ambiente

### Obrigatorias para subir a aplicacao com seguranca

- `ConnectionStrings__DefaultConnection`
- `Jwt__SecretKey`
- `Redis__ConnectionString`
- `Redis__InstanceName`

### Recomendadas conforme o ambiente

- `Elasticsearch__Uri`
- `RabbitMQ__Host`
- `RabbitMQ__Username`
- `RabbitMQ__Password`
- `Quartz__TopSellingGamesCron`
- `OpenTelemetry__CollectorEndpoint`
- `AWS__Region`
- `AWS__Logging__LogGroup`
- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`

## Desenvolvimento local

### Pre-requisitos

- .NET 8 SDK
- Docker
- Um SQL Server acessivel pela API
- Um Redis acessivel pela API
- Um Elasticsearch acessivel pela API
- Um RabbitMQ acessivel pela API

### appsettings

O arquivo `FCG.Catalog/FCG.Catalog.API/appsettings.json` ja contem parte da estrutura de configuracao esperada, incluindo Redis, Quartz, RabbitMQ, AWS e OpenTelemetry.

Segredos e endpoints reais nao devem ser versionados. Para desenvolvimento local, prefira user secrets ou variaveis de ambiente.

### Executando localmente

Exemplo de variaveis minimas em PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=FCGCatalog;User Id=sa;Password=<SENHA>;TrustServerCertificate=True;Encrypt=False;"
$env:Jwt__SecretKey = "<JWT_SECRET>"
$env:Redis__ConnectionString = "localhost:6379,abortConnect=false"
$env:Redis__InstanceName = "fcg:catalog:"
$env:Elasticsearch__Uri = "http://localhost:9200"
$env:RabbitMQ__Host = "localhost"
$env:RabbitMQ__Username = "guest"
$env:RabbitMQ__Password = "guest"
```

Depois disso, execute a API a partir de `FCG.Catalog/FCG.Catalog.API`.

## Build com Docker

A partir da raiz do repositorio:

### Imagem da aplicacao

```powershell
docker build --target runtime -t catalogapi:latest -f FCG.Catalog/FCG.Catalog.API/Dockerfile FCG.Catalog
```

### Imagem de migrations

```powershell
docker build --target migrations -t catalogapi:migrations -f FCG.Catalog/FCG.Catalog.API/Dockerfile FCG.Catalog
```

### Executando o container da API

```powershell
docker run --rm -p 8080:8080 `
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=FCGCatalog;User Id=sa;Password=<SENHA>;TrustServerCertificate=True;Encrypt=False;" `
  -e Jwt__SecretKey="<JWT_SECRET>" `
  -e Redis__ConnectionString="host.docker.internal:6379,abortConnect=false" `
  -e Redis__InstanceName="fcg:catalog:" `
  -e Elasticsearch__Uri="http://host.docker.internal:9200" `
  -e RabbitMQ__Host="host.docker.internal" `
  -e RabbitMQ__Username="guest" `
  -e RabbitMQ__Password="guest" `
  catalogapi:latest
```

### Executando migrations em container

```powershell
docker run --rm `
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=FCGCatalog;User Id=sa;Password=<SENHA>;TrustServerCertificate=True;Encrypt=False;" `
  -e Jwt__SecretKey="<JWT_SECRET>" `
  -e Redis__ConnectionString="host.docker.internal:6379,abortConnect=false" `
  -e Redis__InstanceName="fcg:catalog:" `
  catalogapi:migrations
```

## Kubernetes

Os manifests do servico estao em `FCG.Catalog/k8s/`:

- `catalogapi-namespace.yaml`
- `catalogapi-configmap.yaml`
- `secret-template.yaml`
- `catalogapi-deployment.yaml`
- `catalogapi-service.yaml`
- `catalogapi-migration-job.yaml`

### ConfigMap atual

O ConfigMap versionado hoje define:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://+:8080`
- `AWS_EC2_METADATA_DISABLED=true`
- `AWS_REGION=us-east-1`
- `AWS_DEFAULT_REGION=us-east-1`
- `Redis__InstanceName=fcg:catalog:`
- `Redis__GameCacheTtlMinutes=60`
- `Redis__TopSellingCacheTtlMinutes=120`
- `Quartz__TopSellingGamesCron=0 30 * ? * *`

### Secret template atual

O template versionado hoje contempla:

- `ConnectionStrings__DefaultConnection`
- `Redis__ConnectionString`
- `Jwt__SecretKey`

Se o ambiente Kubernetes tambem depender de RabbitMQ, Elasticsearch ou OTLP com valores diferentes dos defaults do codigo, inclua essas chaves em `Secret` ou `ConfigMap` conforme a sensibilidade do dado.

### Aplicando os manifests

```powershell
kubectl apply -f FCG.Catalog/k8s/catalogapi-namespace.yaml
kubectl apply -f FCG.Catalog/k8s/catalogapi-configmap.yaml
kubectl apply -f FCG.Catalog/k8s/secret-template.yaml
kubectl apply -f FCG.Catalog/k8s/catalogapi-deployment.yaml
kubectl apply -f FCG.Catalog/k8s/catalogapi-service.yaml
```

### Rodando migrations no cluster

```powershell
kubectl apply -f FCG.Catalog/k8s/catalogapi-migration-job.yaml
```

## Boas praticas

- Nao versione segredos reais no repositorio.
- Prefira `Secret`, cofre de segredos ou variaveis injetadas pela esteira.
- Ao mudar nomes de chaves de configuracao, atualize README, manifests e `appsettings` juntos.
- Ao adicionar nova dependencia de infraestrutura, documente a chave de configuracao e o valor padrao, quando existir.
