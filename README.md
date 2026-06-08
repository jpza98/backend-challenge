# API de Reserva de Passagens Aéreas (StarCorp Travel)

Esse é o back-end de um sistema de reserva de passagens aéreas. O fluxo é o que você esperaria: o cliente pesquisa voos, escolhe um, monta uma reserva com um ou mais passageiros, paga e, se precisar, cancela. O preço e o reembolso mudam de acordo com a classe tarifária e a forma de pagamento.

## Stack

| Item | Tecnologia |
|------|------------|
| Linguagem | C# |
| Plataforma | .NET 10 |
| Acesso a dados | Dapper (SQL puro, sem outros ORMs) |
| Banco | SQL Server |
| API | ASP.NET Core (REST) |
| Testes | xUnit |

## Arquitetura

Separei o projeto em três camadas, com as dependências sempre apontando para o centro. O `Core` não conhece ninguém e é onde mora o domínio.

```
src/
  StarCorp.FlightBooking.Api/            → Controllers, Program.cs, configuração HTTP
  StarCorp.FlightBooking.Core/           → Domínio: Models, Enums, DTOs, Interfaces, Services
  StarCorp.FlightBooking.Infrastructure/ → Repositories Dapper, acesso ao SQL Server
tests/
  StarCorp.FlightBooking.Tests/          → Testes xUnit das regras de negócio
db/
  schema.sql                             → DDL
  seed.sql                               → dados de exemplo
```

Deixei as regras de negócio (cálculo de preço e política de cancelamento) em `Core/Services` como serviços puros, sem nenhuma dependência de banco. Fiz isso de propósito: assim dá para testar essas regras em unidade sem precisar subir SQL Server, e é justamente nelas que está a maior parte do valor dos testes.

## Como rodar

### 1. Banco

Com uma instância de SQL Server disponível, rode os scripts nesta ordem:

```bash
sqlcmd -S localhost -U sa -P sua_senha -i db/schema.sql
sqlcmd -S localhost -U sa -P sua_senha -i db/seed.sql
```

Se quiser subir um SQL Server rápido via Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Sua@Senha123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

Depois é só ajustar a connection string em `src/StarCorp.FlightBooking.Api/appsettings.json`.

### 2. Aplicação

```bash
dotnet run --project src/StarCorp.FlightBooking.Api
```

A API sobe em `http://localhost:5129` (perfil `http`) ou `https://localhost:7095` (perfil `https`). Em desenvolvimento, a especificação OpenAPI fica disponível em `/openapi/v1.json`. Para explorar a API sem ter que montar requisição na mão, dá para importar a collection do Postman (mais sobre isso na seção [Testando a API](#testando-a-api-postman)).

### 3. Testes

```bash
dotnet test
```

## Endpoints

### Voos

| Verbo | Rota | Descrição |
|-------|------|-----------|
| GET | `/api/flights` | Busca voos com filtros e paginação |
| GET | `/api/flights/{id}` | Detalhe de um voo |
| GET | `/api/flights/{id}/price` | Simula o preço de um voo (extra, fora do escopo) |

Os filtros de `/api/flights` são todos opcionais e podem ser combinados: `origin`, `destination`, `date`, `fareClass`, `passengers`, `minPrice`, `maxPrice`, `page` (padrão 1) e `pageSize` (padrão 10). A resposta vem paginada, com `page`, `pageSize`, `total`, `totalPages` e `items`.

### Reservas

| Verbo | Rota | Descrição |
|-------|------|-----------|
| POST | `/api/bookings` | Cria a reserva (sem pagamento) e retorna o breakdown do preço |
| GET | `/api/bookings/{id}` | Detalhe da reserva |
| GET | `/api/bookings/code/{code}` | Busca por código de reserva (extra) |
| GET | `/api/bookings/customer/{customerId}` | Reservas de um cliente (extra) |
| POST | `/api/bookings/{id}/payment` | Processa o pagamento e fecha o total |
| POST | `/api/bookings/{id}/cancel` | Cancela e calcula o reembolso |

## Testando a API (Postman)

Deixei uma collection pronta na pasta `postman/` (`StarCorp-Travel.postman_collection.json`) que cobre o fluxo inteiro de ponta a ponta. É só importar no Postman (File, Import) e clicar em Run collection que ela roda tudo em sequência.

Ela está dividida em três pastas. A de Voos faz a busca paginada, a busca com filtros, o detalhe e a simulação de preço. A de Fluxo de reserva cria a reserva, consulta por id e por código, paga no Pix, lista as reservas do cliente e cancela. O `bookingId` e o `bookingCode` são capturados automaticamente na criação e reaproveitados nas requests seguintes, então você não precisa copiar nada na mão. A de Casos de erro cobre cliente inativo (`422`) e recursos inexistentes (`404`), já com as asserções automáticas.

Um detalhe que vale notar: como o cancelamento roda logo depois do pagamento, ele acaba exercitando a regra especial das 24h (reembolso integral) sem precisar montar nenhum cenário à parte.

A `baseUrl` da collection aponta para `http://localhost:5129`. Se você rodar no perfil https, é só trocar a variável para `https://localhost:7095`.

## Regras de negócio

### Composição do preço

```
Subtotal       = preço base do voo × multiplicador da classe × nº de passageiros
+ Impostos     = 8% do subtotal + R$ 45 fixos por passageiro
+ Taxa serviço = 5% sobre (subtotal + impostos)
± Ajuste do método de pagamento
= TOTAL
```

### Multiplicador por classe

| Classe | Multiplicador |
|--------|---------------|
| Econômica | 1,0× |
| Executiva | 2,5× |

### Ajuste por método de pagamento

| Método | Ajuste |
|--------|--------|
| Cartão de Crédito | +3% |
| Pix | −5% |
| Boleto | +1% |

### Política de cancelamento

| Classe | Mais de 7 dias | 2 a 7 dias | Menos de 2 dias |
|--------|----------------|------------|-----------------|
| Econômica | 100% | 50% | 0% |
| Executiva | 100% | 75% | 25% |

Tem uma regra especial por cima disso: cancelamento em até 24h depois do pagamento dá reembolso integral, não importa o que a tabela acima diga.

## Decisões técnicas

Sobre a modelagem das classes tarifárias, o enunciado deixou em aberto e eu fui de enum em vez de herança ou tabela separada. Como só existem duas classes e o que muda entre elas é só comportamento (o multiplicador de preço e o percentual de reembolso), modelei `FareClass` como enum e coloquei as regras nos serviços de domínio (`PricingService` e `CancellationService`). Herança ou single-table aqui só ia adicionar cerimônia sem ganho nenhum, já que não existe atributo próprio de cada classe que justifique uma entidade separada. Se um dia aparecer uma classe com estrutura própria (assento diferente, bagagem, esse tipo de coisa), o caminho natural seria um Strategy por classe, e os serviços já deixam isso fácil.

Separei o pagamento da criação da reserva de propósito. O `POST /api/bookings` só cria a reserva como pendente e já devolve o breakdown até a taxa de serviço. O ajuste da forma de pagamento e o total final fecham mesmo é no `POST /api/bookings/{id}/payment`. Fora ser o que o enunciado pede, é isso que faz a regra das 24h funcionar de verdade: o reembolso integral depende do momento do pagamento (`PaidAt`), e esse momento só existe porque o pagamento é um passo separado. Se ele estivesse embutido na criação, não dava para contar essas 24h direito.

Ainda sobre o preço, o item 4.2 pede o "preço completo" na criação, mas o ajuste da forma de pagamento (seção 5.1) só dá para saber na hora de pagar. Resolvi a ambiguidade assim: o Create calcula e retorna subtotal mais impostos mais taxa de serviço, e o ajuste com o total final ficam para o endpoint de pagamento. O cálculo base é o mesmo nos dois lugares, então não tem lógica duplicada.

Usei Dapper com SQL escrito na mão. Além de ser exigência do desafio, me dá controle total sobre as queries (paginação, filtros dinâmicos, baixa de assento). Os repositórios escondem todo o SQL atrás de interfaces declaradas no `Core`.

Nos códigos HTTP segui uma linha simples: `400` para entrada inválida, `404` para recurso que não existe, `409` para conflito de estado (sem assento, reserva já paga) e `422` para regra de negócio violada (cliente inativo, cancelamento não permitido).

O cliente é identificado pelo CPF. Na hora de criar a reserva eu busco o cliente por CPF e crio caso ele não exista. Se existir mas estiver inativo (`IsActive = false`), a reserva é barrada com `422`.

## O que eu faria com mais tempo

A primeira coisa seria mover a paginação para o banco. Hoje a busca de voos traz os resultados e pagina em memória com `Skip/Take`, e o certo seria empurrar isso para o SQL com `OFFSET/FETCH` mais um `COUNT`, para não trazer mais linha do que precisa.

Depois, transações. Criar reserva e dar baixa no assento, ou pagar e atualizar o status, hoje são chamadas separadas, e sob concorrência dá para vender o mesmo assento duas vezes. Eu envolveria essas operações em transação e colocaria algum controle de concorrência no estoque de assentos.

Colocaria também idempotência no pagamento, com uma chave no `POST .../payment`, para evitar cobrança dobrada num eventual retry.

Nos testes, hoje eu cubro as regras de negócio em unidade. Com mais tempo subiria um SQL Server efêmero com Testcontainers para testar os repositórios e o fluxo de ponta a ponta.

Por último, tiraria as poucas magic strings que sobraram (status como `"Scheduled"` e `"Confirmed"`), deixando tudo em enum, e adicionaria autenticação, que ficou de fora do escopo do desafio.
