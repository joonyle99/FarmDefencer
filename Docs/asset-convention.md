
# 유니티 에셋 네이밍 규칙

이 문서는 유니티 프로젝트에서 사용하는 에셋의 네이밍 규칙을 정의합니다.
이 지침을 따르면 프로젝트를 깔끔하고 체계적으로 관리할 수 있으며, 팀 간의 협업이 더욱 원활해집니다.

## 일반 규칙

- 모든 에셋 이름은 **영문**으로 작성합니다.
- `/`, `\`, `@`, `#` 등 특수 문자는 사용하지 않습니다.
- **snake_case**를 사용하여 이름을 작성합니다.
- 에셋에 대한 명확하고 설명적인 이름을 사용합니다.

> **snake_case**: 단어 사이에 언더스코어를 사용합니다 (예: `player_character`, `enemy_tank`)

## 예시

head_clean_option
![head_clean_option](https://github.com/user-attachments/assets/e4912e91-c813-4e60-a290-edbd4eab069d)
head_cracked_option
![head_cracked_option](https://github.com/user-attachments/assets/990a3657-1d95-45dd-8b1e-e0ab8e3ca1e5)

## 버전 관리

- 버전 관리가 필요한 경우, 에셋 이름 끝에 버전 번호를 포함시킵니다.
  - 예시: `enemy_tank_v1.prefab`, `skybox_desert_v1.1.png`

## 폴더 구조

- 일관된 폴더 구조를 유지하여 에셋을 보관합니다.
  ```
  Assets/
    ├── External Asset/
    ├── Project Resources/
    │   ├── Animations/
    │   ├── Prefabs/
    │   ├── Materials/
    │   └── Images/
    │   └── Sounds/
    ├── Resources/
    ├── Scenes/
    ├── Scripts/
    .
    .
    .
  ```
