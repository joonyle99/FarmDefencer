
## 에셋 및 리소스 네이밍 규칙

이 문서는 유니티 프로젝트에서 사용하는 에셋 및 리소스의 네이밍 규칙을 정의합니다.  
이 지침을 따르면 프로젝트를 깔끔하고 체계적으로 관리할 수 있으며, 팀 간의 협업이 더욱 원활해집니다.

### 폴더 구조

- **Title Case**를 사용합니다. (ex. `Project Assets`)
- 폴더 구조에 따라 에셋 및 리소스 파일의 유형을 명확히 구분합니다.
  - 예시: `Textures` 폴더에 있는 파일은 모두 텍스처 파일이므로 파일명에 `tex_` 프리픽스를 붙일 필요가 없습니다.
  ```
  Assets/
    ├── External Assets/
    ├── Project Assets/      // 유니티 내에서 제작하는 파일 (ex. 애니메이션, 머터리얼, 프리팹, 스크립트, 씬, 게임 오브젝트 등)
    │   ├── Animations/
    │   |   ├── AnimatorControllers/
    │   ├── Materials/
    │   |   ├── Shaders/  
    │   ├── Meshs/
    │   ├── Prefabs/
    ├── Project Resources/   // 게임 외부에서 가져오거나 별도로 제작한 파일 (ex. 텍스처, 사운드, UI 이미지 등)
    │   └── Sounds/
    │   └── Texture/
    │   └── UI/
    ├── Resources/           // 런타임으로 불러오기 위한 리소스 (Unity Resources API)
    ├── Scenes/
    ├── Scripts/
    .
    .
    .
  ```

### 일반 규칙

- 에셋 및 리소스에 대한 명확하고 설명적인 이름을 사용합니다.
- 모든 에셋 및 리소스의 이름은 **영문**으로 작성합니다.

### 에셋 규칙

- `에셋`은 유니티 내에서 제작하는 파일을 말합니다. (ex. 애니메이션, 머터리얼, 프리팹, 스크립트, 씬, 게임 오브젝트 등)
- **PascalCase**를 사용하여 이름을 작성합니다. (ex. `SampleScene.scene`, `TilePalette.prefab`)

### 리소스 규칙

- `리소스`는 게임 외부에서 가져오거나 별도로 제작한 파일을 말합니다. (ex. 텍스처, 사운드, UI 이미지 등)
- **snake_case**를 사용하여 이름을 작성합니다. (ex. `player_character.png`, `background_music.wav`)

![head_clean_option](https://github.com/user-attachments/assets/e4912e91-c813-4e60-a290-edbd4eab069d) head_clean_option.png
![head_cracked_option](https://github.com/user-attachments/assets/990a3657-1d95-45dd-8b1e-e0ab8e3ca1e5) head_cracked_option.png

