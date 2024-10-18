## 팜디펜서 코딩 컨벤션

### 1. 개요

이 문서는 팜디펜서 프로젝트에서 C# 코드를 작성할 때 따를 코딩 컨벤션을 정의합니다.  
일관된 코드 스타일은 협업과 유지보수를 쉽게 만들고, 코드 품질을 향상시킵니다.  
아래 규칙을 준수해 팀 내에서 통일된 코딩 스타일을 유지합니다.

---

### 2. 네이밍 규칙

#### 2.1 변수 및 필드

- 비공개 필드 (private): `_camelCase`를 사용하며, 접두어로 `_`를 붙입니다.
  - 예시: `_currentScore`, `_maxValue`

<br>

- 보호된 필드 (protected): `camelCase`를 사용합니다.
  - 예시: `targetAttackCount`, `attackCountRange`

<br>

- 공개 필드 (public): `PascalCase`를 사용하며, 프로퍼티(Property) 형식을 선호합니다.
  - 예시: `MonsterName`, `MaxHp`

<br>

- 로컬 변수 및 매개 변수: `camelCase`를 사용하여 작성합니다.
  - 예시: `userName`, `totalCount`

<br>

- 전역 상수: `UPPER_CASE`를 사용하며, 각 단어는 언더스코어(`_`)로 구분합니다. `const` 키워드를 사용합니다.
  - 예시: `MAX_RETRY_COUNT`, `DEFAULT_TIMEOUT`

#### 2.2 메서드 및 함수

- 메서드: `PascalCase`를 사용합니다.
  - 예시: `CalculateScore()`, `GetUserName()`

#### 2.3 클래스 및 인터페이스

- 클래스: `PascalCase`를 사용하며, 명사 또는 명사구로 작성합니다.
  - 예시: `PlayerManager`, `ScoreCalculator`

<br>

- **인터페이스**: `I` 접두어를 붙이고 `PascalCase`를 사용합니다.
  - 예시: `IUserService`, `ILogger`

#### 2.4 열거형 (Enum)

- 열거형 이름: `PascalCase`를 사용합니다.
  - 예시: `GameState`, `UserRole`

<br>

- 열거형 멤버: `PascalCase`를 사용합니다.
  - 예시: `Playing`, `Paused`, `Stopped`

---

### 3. 코드 형식

> Tip: 코드 형식을 맞추는 데 시간을 절약하고 일관성을 유지하기 위해,
> Visual Studio에서 Ctrl + K, Ctrl + D 단축키를 사용하여 자동으로 코드 포맷팅을 적용할 수 있습니다.
> 이 기능을 적극적으로 사용하여 코드를 깔끔하게 유지하세요.

#### 3.1 빈 줄

논리적인 코드 블록 사이에 빈 줄을 추가하여 가독성을 높입니다.
- 메서드와 메서드 사이
- 클래스 내 필드와 메서드 사이

#### 3.2 공백 사용

```csharp
// 콤마(,)와 콜론(:) 뒤에 공백을 추가합니다.
public void Run(int speed, int distance)
```

```csharp
// 연산자 양쪽에 공백을 넣습니다.
int result = value + 10;
```

#### 3.3 한 줄에 하나의 문장

```csharp
// 한 줄에 하나의 문장만 작성하여 가독성을 높입니다
var total = CalculateTotal();
total += 10;
```

---

### 4. 접근 제어자

모든 클래스, 메서드, 속성, 필드에 명시적인 접근 제어자를 사용합니다.  
기본적으로 private을 사용하고, 필요에 따라 public, protected 등을 명시적으로 지정합니다.

```csharp
public class Player
{
    private int _score;
    protected int protectedScore;
    public int Score => _score;
}
```

---

### 5. 주석 규칙

#### 5.1 XML 주석

클래스, 메서드, 프로퍼티에 대한 설명은 XML 주석을 사용하여 작성합니다.

```csharp
/// <summary>
/// 플레이어의 점수를 계산합니다.
/// </summary>
public int CalculateScore(Player player)
{
    // 로직
}
```

아래와 같은 파라미터 주석은 선택적으로 작성합니다.
```csharp
/// <param name="player">플레이어 객체</param>
/// <returns>계산된 점수</returns>
```

#### 5.2 단일 줄 주석

중요한 로직이나 이해하기 어려운 부분에는 단일 줄 주석을 사용하여 코드를 설명합니다.

```csharp
// 플레이어의 점수를 초기화
_score = 0;
```
