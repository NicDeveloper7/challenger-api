# Challenger API

Desenvolvido por Nicholas Balbino 

API para gestão de entregadores, motos e locações. Este guia explica como rodar o projeto, exemplos de requisições, quais eventos Kafka são publicados e como testá-los. No final, há um checklist para validar o contrato com o Swagger da Mottu.

## Requisitos

- Docker Desktop (para subir Kafka + Kafdrop)
- .NET SDK instalado (para rodar a API localmente)
- PostgreSQL acessível em `localhost:5432` com credenciais do `appsettings.json`:
  - Database: `challenger_db`
  - Username: `postgres`
  - Password: `root`

Observação: O docker-compose deste repositório sobe somente Kafka/Zookeeper/Kafdrop. O Postgres deve estar disponível localmente ou em outro container (veja abaixo uma opção opcional por Docker).

## Como rodar

1) Subir infraestrutura de mensageria (Kafka + Zookeeper + Kafdrop)

```powershell
# Na pasta raiz do repositório
docker compose up -d
# Acesse o Kafdrop: http://localhost:19000
```

2) Banco de dados PostgreSQL

- Já tem Postgres local? Garanta que a conexão do `appsettings.json` está correta e o serviço está ativo.
- Alternativa via Docker (opcional):

```powershell
# Opcional: subir Postgres em container
docker run --name postgres-challenger -e POSTGRES_PASSWORD=root -e POSTGRES_DB=challenger_db -p 5432:5432 -d postgres:15
```

3) Aplicar migrations EF Core (criar/atualizar o schema)

- Via tarefa do VS Code: execute a task "apply-ef-migrations".
- Ou via CLI:

```powershell
# Na raiz do repo
 dotnet ef database update -p Challenger.Infra/Challenger.Infra.csproj -s Challenger.Api/Challenger.Api.csproj
```

4) Rodar a API

- Via tarefa do VS Code: "run-api"
- Ou via CLI:

```powershell
 dotnet run --project .\Challenger.Api\Challenger.Api.csproj
# Swagger local (HTTP): http://localhost:5115/swagger
```

5) Armazenamento de CNH (upload)

- Por padrão, o provedor de storage é Local, salvando arquivos em `c:/data/challenger-storage` (configure em `appsettings.json > Storage`).
- Para usar MinIO, ajuste `Storage.Provider` para `Minio` e configure `Storage:Minio`.

---

## Exemplos de requisições (curl)

Observações:
- Em Windows, o `curl` já está disponível. Use aspas duplas e JSON no `-d`.
- A base URL padrão de desenvolvimento é `http://localhost:5115`.

### Motorcycles

- Criar moto

```powershell
curl -X POST "http://localhost:5115/motorcycles" ^
  -H "Content-Type: application/json" ^
  -d "{\"year\":2024,\"model\":\"Honda CG 160\",\"plate\":\"ABC1D23\"}"
```

- Listar motos (todas ou filtrando por placa)

```powershell
curl "http://localhost:5115/motorcycles"
# com filtro:
curl "http://localhost:5115/motorcycles?plate=ABC1D23"
```

- Atualizar placa

```powershell
# substitua {id}
curl -X PUT "http://localhost:5115/motorcycles/{id}" ^
  -H "Content-Type: application/json" ^
  -d "{\"plate\":\"XYZ9Z99\"}"
```

- Remover moto

```powershell
curl -X DELETE "http://localhost:5115/motorcycles/{id}"
```

### Couriers

- Cadastrar entregador

```powershell
curl -X POST "http://localhost:5115/couriers" ^
  -H "Content-Type: application/json" ^
  -d "{\"name\":\"João Silva\",\"email\":\"joao@example.com\",\"password\":\"Senha@123\",\"cnhNumber\":\"1234567890\",\"cnhType\":\"A\",\"cnpj\":\"12345678000199\",\"birthDate\":\"1990-01-15T00:00:00Z\"}"
```

- Upload da CNH (somente PNG ou BMP)

```powershell
# substitua {id} e o caminho do arquivo
curl -X POST "http://localhost:5115/couriers/{id}/cnh" ^
  -H "Accept: application/json" ^
  -F "file=@C:\\caminho\\para\\cnh.png;type=image/png"
```

### Rentals

- Criar locação

```powershell
curl -X POST "http://localhost:5115/rentals" ^
  -H "Content-Type: application/json" ^
  -d "{\"courierId\":\"{guid_do_entregador}\",\"planDays\":7,\"expectedEndDate\":\"2025-10-30T00:00:00Z\"}"
```

- Consultar locação por id

```powershell
curl "http://localhost:5115/rentals/{id}"
```

- Finalizar locação (informar devolução)

```powershell
curl -X PUT "http://localhost:5115/rentals/{id}/return" ^
  -H "Content-Type: application/json" ^
  -d "{\"actualEndDate\":\"2025-10-25T00:00:00Z\"}"
```

---

## Eventos Kafka: o que é publicado e quando

- Evento: `MotorcycleCreatedEvent`
- Tópico: `motorcycle_created`
- Publicado: após a criação de uma moto com sucesso (na `MotorcycleService.CreateAsync`, depois de gravar no banco).
- Payload (exemplo):

```json
{
  "Id": "8b8a0c0c-4a11-4e2a-9b28-4a0e0e5b83a1",
  "Year": 2024,
  "Model": "Honda CG 160",
  "Plate": "ABC1D23",
  "CreatedAt": "2025-10-19T12:34:56Z"
}
```

- Consumidor: `MotorcycleCreatedConsumer` (habilitado quando `Kafka.Enabled=true`).
  - Regra de negócio: quando `Year == 2024`, cria um registro em `HighlightedMotorcycles` (tabela) com os dados da moto.

---

## Como testar mensagens Kafka

1) Habilite o Kafka na API (arquivo `Challenger.Api/appsettings.json` ou `appsettings.Development.json`):

```json
{
  "Kafka": {
    "Enabled": true,
    "BootstrapServers": "localhost:9092",
    "ClientId": "challenger-api",
    "TopicMotorcycleCreated": "motorcycle_created",
    "ConsumerGroupId": "challenger-api-group"
  }
}
```

2) Suba o Kafka e Kafdrop

```powershell
# na raiz do repo
 docker compose up -d
# Kafdrop: http://localhost:19000
```

3) Inicie a API (veja seção "Como rodar") e crie uma moto (veja exemplos de curl).

4) Verifique a mensagem:

- Via Kafdrop: acesse o tópico `motorcycle_created` e confira as mensagens.
- Via CLI dentro do container Kafka (alternativa):

```powershell
# Abra um shell dentro do container Kafka
 docker compose exec kafka bash
# já dentro do container, execute o consumer (com histórico)
 kafka-console-consumer --bootstrap-server kafka:29092 --topic motorcycle_created --from-beginning
```

Você deverá ver o JSON do evento publicado após a criação da moto. Se o ano for 2024, o consumidor gravará um destaque em `HighlightedMotorcycles` (consulte o banco para confirmar).

---

## Testar com o Swagger da Mottu (conferir contrato)

- Swagger local: http://localhost:5115/swagger
- Importe/abra o Swagger oficial da Mottu e compare os contratos seguintes:

Checklist de compatibilidade:
- Endpoints e métodos:
  - POST `/motorcycles` (criar)
  - GET `/motorcycles` com `?plate=` (listar/filtrar)
  - PUT `/motorcycles/{id}` (atualizar placa)
  - DELETE `/motorcycles/{id}` (remover)
  - POST `/couriers` (criar entregador)
  - POST `/couriers/{id}/cnh` (upload CNH via multipart/form-data; campo `file` — formatos aceitos: PNG ou BMP)
  - POST `/rentals` (criar locação)
  - GET `/rentals/{id}` (detalhe da locação)
  - PUT `/rentals/{id}/return` (devolução)

- Modelos/payloads esperados:
  - CreateMotorcycleRequest: `{ year:int, model:string, plate:string }`
  - UpdateMotorcyclePlateRequest: `{ plate:string }`
  - CreateCourierRequest: `{ name, email, password, cnhNumber, cnhType, cnpj, birthDate }`
  - CreateRentalRequest: `{ courierId:guid, planDays:int, expectedEndDate:datetime }`
  - ReturnRentalRequest: `{ actualEndDate:datetime }`

- Códigos de resposta e erros:
  - 201 Created nas criações (Location/CreatedAt retornando o recurso)
  - 200 OK para consultas/updates
  - 204 No Content para delete bem-sucedido
  - 400 Bad Request para validações (ex.: campos obrigatórios, formato inválido de arquivo)
  - 404 Not Found quando recurso não existe
  - 409 Conflict quando há violação de unicidade (ex.: placa duplicada) ou regras de negócio (ex.: deletar moto com locações)

Caso note diferenças do contrato esperado, anote o endpoint, payload e resposta divergentes para ajuste posterior.

---

## Dicas e troubleshooting

- Falha para conectar ao Postgres: verifique se o serviço está ativo e a porta 5432 está livre (ou ajuste a connection string).
- Mensagens Kafka não aparecem: confirme `Kafka.Enabled=true`, suba o docker-compose e verifique o tópico no Kafdrop.
- Upload CNH: somente `image/png` ou `image/bmp` são aceitos; confira o `Content-Type` no upload.
- Permissões de pasta no Windows: crie `c:/data/challenger-storage` (ou ajuste no `appsettings.json`).

---

## Scripts úteis (opcional)

- Reaplicar migrations do zero (cuidado em produção):
```powershell
 dotnet ef database update 0 -p Challenger.Infra/Challenger.Infra.csproj -s Challenger.Api/Challenger.Api.csproj
 dotnet ef database update -p Challenger.Infra/Challenger.Infra.csproj -s Challenger.Api/Challenger.Api.csproj
```

- Rodar build/restore via tarefa do VS Code: `dotnet-restore-build`.

---

Feito para facilitar a rotina de testes.

---

## Testes (unitários e integração)

Antes de rodar testes, certifique-se de que a API não está em execução (evita erro de arquivo bloqueado durante o build dos testes):

```powershell
Get-Process Challenger.Api -ErrorAction SilentlyContinue | Stop-Process -Force
# ou
# taskkill /IM Challenger.Api.exe /F
```

- Rodar todos os testes:

```powershell
dotnet test .\Challenger.Tests\Challenger.Tests.csproj -c Debug
```

- Somente testes de integração (filtrando por nome totalmente qualificado):

```powershell
dotnet test .\Challenger.Tests\Challenger.Tests.csproj -c Debug --filter "FullyQualifiedName~Integration"
```

- Somente testes unitários (exclui integração):

```powershell
dotnet test .\Challenger.Tests\Challenger.Tests.csproj -c Debug --filter "FullyQualifiedName!~Integration"
```

- Rodar uma classe específica:

```powershell
dotnet test .\Challenger.Tests\Challenger.Tests.csproj -c Debug --filter "FullyQualifiedName=Challenger.Tests.Integration.MotorcyclesIntegrationTests"
```

- Rodar um método específico:

```powershell
dotnet test .\Challenger.Tests\Challenger.Tests.csproj -c Debug --filter "FullyQualifiedName=Challenger.Tests.Integration.MotorcyclesIntegrationTests.Create_And_Get_List_Should_Work"
```

- Via Test Explorer do VS Code: abra a aba Testing e clique em "Run All Tests" ou rode testes individuais. Se falhar o build por arquivo bloqueado, pare a API e tente novamente.



