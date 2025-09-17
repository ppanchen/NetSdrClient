# NetSdrClient — Лабораторні з реінжинірингу (8×) + CI/SonarCloud
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=coverage)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=bugs)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)


Цей репозиторій використовується для курсу **реінжиніринг ПЗ**. 
Мета — провести комплексний реінжиніринг спадкового коду NetSdrClient, включаючи рефакторинг архітектури, покращення якості коду, впровадження сучасних практик розробки та автоматизацію процесів контролю якості через CI/CD пайплайни.

---

## Вимоги та швидкий старт

**Необхідно:**
- .NET 8 SDK
- Публічний GitHub-репозиторій
- Обліковка SonarCloud (організація прив’язана до GitHub)

**1) Підключити SonarCloud**
- На SonarCloud створити проект з цього репозиторію (*Analyze new project*).
- Згенерувати **user token** і додати в репозиторій як секрет **`SONAR_TOKEN`** (*Settings → Secrets and variables → Actions*).
- У SonarCloud **вимкнути Automatic Analysis** (використовуємо CI-аналіз).

**2) Увімкнути CI (GitHub Actions)**
- Файл `.github/workflows/ci.yml` має запускатися на `pull_request` **і** на `push` у `main`/`master`.
- Приклад сонар-кроку в workflow:
  ```yaml
  - name: SonarCloud scan
    if: github.event_name == 'pull_request' || github.ref == 'refs/heads/main'
    uses: SonarSource/sonarqube-scan-action@v5
    env:
      SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
    with:
      args: >
        -Dsonar.host.url=https://sonarcloud.io
        -Dsonar.organization=ppanchen
        -Dsonar.projectKey=ppanchen_NetSdrClient
        -Dsonar.cs.opencover.reportsPaths=**/coverage.xml
        -Dsonar.qualitygate.wait=true
  ```

**3) Gated merge**
- *Settings → Branches → Add rule (`main`/`master`)* → **Require a pull request before merging** + **Require status checks to pass**.
- Після першого успішного ранну на `main` у списку з’являться чеки (job CI та **SonarCloud Code Analysis/Quality Gate**).

---

## Структура 8 лабораторних

> Кожна робота — **через Pull Request**. У PR додати короткий опис: *що змінено / як перевірити / ризики*.

### Лаба 1 — Підключення SonarCloud і CI

**Мета:** створити проект у SonarCloud, підключити GitHub Actions, запустити перший аналіз.

**Необхідно:**
- .NET 8 SDK
- Публічний GitHub-репозиторій
- Обліковка SonarCloud (організація прив’язана до GitHub)

**1) Підключити SonarCloud**
- На SonarCloud створити проект з цього репозиторію (*Analyze new project*).
- Згенерувати **user token** і додати в репозиторій як секрет **`SONAR_TOKEN`** (*Settings → Secrets and variables → Actions*).
- У SonarCloud **вимкнути Automatic Analysis** (використовуємо CI-аналіз).

**Кроки:**
- Імпортувати репозиторій у SonarCloud, додати `SONAR_TOKEN`.
- Додати/перевірити `.github/workflows/ci.yml` з тригерами на PR і push у основну гілку.
- **Вимкнути Automatic Analysis** в проєкті.
- Перевірити **PR-декорацію** (вкладка *Checks* у PR).

**Здати:** посилання на PR із зеленим аналізом, скрін Quality Gate, скрін бейджів у README.

---

### Лаба 2 — Code Smells через PR + “gated merge”

**Мета:** виправити **5–10** зауважень Sonar (bugs/smells) без зміни поведінки.

**Кроки:**
- Дрібними комітами виправити знайдені Sonar-проблеми у `NetSdrClientApp`.

**Здати:** PR із “зеленими” required checks; скріни змін метрик у Sonar.

---

### Лаба 3 — Тести та покриття

**Мета:** додати **4–6** юніт-тестів; підняти покриття в модулі.

**Кроки:**
- Підключити генерацію покриття (один з варіантів):
  - `coverlet.msbuild`:
    ```bash
    dotnet add NetSdrClientAppTests package coverlet.msbuild
    dotnet add NetSdrClientAppTests package Microsoft.NET.Test.Sdk
    dotnet test NetSdrClientAppTests -c Release       /p:CollectCoverage=true       /p:CoverletOutput=TestResults/coverage.xml       /p:CoverletOutputFormat=opencover
    ```
- У Sonar вказати шлях до звіту:
  ```
  sonar.cs.opencover.reportsPaths=**/coverage.xml
  ```
- За бажанням — явно позначити тести у `sonar-project.properties`:
  ```properties
  sonar.tests=NetSdrClientAppTests
  sonar.test.inclusions=NetSdrClientAppTests/**/*.cs
  sonar.exclusions=**/bin/**,**/obj/**,NetSdrClientAppTests/**
  ```

**Здати:** PR із новими тестами, скрін Coverage у Sonar, шлях до звіту в артефактах/логах CI.

---

### Лаба 4 — Дублікати через SonarCloud (без jscpd)

**Мета:** зменшити дублікати коду, пройти Quality Gate.

**Кроки:**
- Переглянути **Measures → Duplications** у Sonar і **Checks → SonarCloud** у PR.
- Прибрати **1–2** найбільші дубльовані фрагменти (рефакторинг/винесення спільного коду).
- Перезапустити CI, перевірити, що *Duplications on New Code* ≤ порога (типово 3%).

**Здати:** PR із зеленим Gate і скрінами “до/після”.

---

### Лаба 5 — Архітектурні правила (NetArchTest)

**Мета:** додати **2–3** архітектурні правила залежностей і зафіксувати їх тестами.

**Кроки:**
- Додати тест-проєктні правила (наприклад, `*.UI` не має залежати від `*.Infrastructure` напряму).
- Переконатися, що порушення **ламає збірку** (червоний PR), а фікс — зеленить.

**Здати:** PR із тестами правил, скрін невдалого прогону (до фіксу) і зеленого (після).

---

### Лаба 6 — Безпечний рефакторинг під тести

**Мета:** виконати **2–3** рефакторинги (без зміни поведінки), підтвердити тестами.

**Кроки:**
- Вибрати локально “важкі” місця (Sonar: Cognitive Complexity, Long Methods).
- Зробити Extract Method / Introduce Parameter Object / Guard Clauses тощо.
- Показати зменшення smells/складності у Sonar.

**Здати:** PR + коротка таблиця метрик “до/після”.

---

### Лаба 7 — Оновлення залежностей (мінімум)

**Мета:** оновити **1–2** NuGet (minor/patch), підтвердити тестами.

**Кроки:**
- `dotnet list NetSdrClient.sln package --outdated --include-transitive`
- Оновити обрані пакети, прогнати тест/сонар.
- Перевірити, що **push у main/master** запускає CI (джоба після мерджу).

**Здати:** PR з оновленням, скрін push-рану після мерджу, нотатки про ризики.

---

### Лаба 8 — Узгоджуємо PR vs push: New Code Definition + фінальні гейти

**Мета:** зробити так, щоб **PR зелений і push зелений** (жодних сюрпризів після мерджу).

**Кроки:**
- У SonarCloud → *Project Settings → New Code*:
  - **Number of days** (напр., 30 дн.) **або**
  - **Previous version** + у сканері `-Dsonar.projectVersion=${{ github.run_number }}`.
- Перевірити, що на `main` Quality Gate зелений (Coverage/Duplications на New Code в нормі).
- (Опц.) Увімкнути **Merge Queue** у GitHub.

**Здати:** скрін *Branches → main* з зеленим Gate + коротке пояснення обраної NCD.

---

## Норми здачі та оцінювання (єдині для всіх лаб)

**Подання:** лише через **Pull Request**.  
**Опис PR:** що зроблено, як перевірити, ризики/зворотна сумісність.  
**Артефакти:** скріни/посилання на Sonar, логи CI, coverage report.  
**Критерій “зелений PR”:** CI пройшов, **Quality Gate** зелений, покриття/дублі в нормі.

---

## Типові граблі → що робити

- **“You are running CI analysis while Automatic Analysis is enabled”**
  Вимкнути *Automatic Analysis* у SonarCloud (використовуємо CI).
- **“Project not found”**
  Перевірити `sonar.organization`/`sonar.projectKey` **точно як у UI**; токен має доступ до org.
- **Покриття не генерується**
  Додати `coverlet.msbuild` або `coverlet.collector`; використовувати формат **opencover**; у Sonar — `sonar.cs.opencover.reportsPaths`.
- **Подвійний аналіз (PR + push)**
  Обмежити умову запуску Sonar: тільки PR **або** `refs/heads/master`.
- **PR зелений, push червоний**
  Перевірити **New Code Definition** (Number of days або Previous version) і довести покриття/дублікації на “new code”.

Успіхів! 🚀