# 🎮 ChrispServer - Game Server API

게임 클라이언트와 통신하는 ASP.NET Core 기반의 게임 서버 API입니다. 

회원가입, 로그인, 캐릭터 장착, 아이템 사용, 미션 진행 등 게임 로직을 REST API로 제공합니다.

## 📦 프로젝트 구조

```plaintext
├── Controllers
│   ├── AccountController.cs
│   ├── CharacterController.cs
│   └── RewardController.cs
│
├── Services
│   ├── AccountService.cs
│   ├── CharacterService.cs
│   ├── MissionService.cs
│   └── MasterHandler.cs
│
├── Middlewares
│   ├── UserAuthMiddleware.cs
│   ├── RedisMemoryDb.cs / InMemoryDb.cs
│
├── Securities
│   ├── RedisAuthService.cs
│   └── RedisKeyMaker.cs
│
├── DbConfigurations
│   └── ConnectionManager.cs
│
├── ResReqModels
│   ├── Request.cs
│   ├── Response.cs
│   └── Result.cs
```


## 🔧 Controller
### AccountController
- POST/Account/CreateAccount : 회원가입
- POST/Account/Login : 로그인

### CharacterController
- POST /Character/EquipCharacter : 캐릭터 변경
- POST /Character/EquipItem : 아이템 장착
- POST /Character/UnequipItem : 아이템 해제
- POST /Character/PlayStatus : 행동 (밥주기, 놀아주기 등)

### RewardController
- POST /Reward/ReceiveMission : 미션 보상 수령 처리


## 🧩 Middleware
### UserAuthMiddleware
- 토큰 유효성 검증 및 TTL 갱신 (로그인 상태 유지)
- Redis 락을 통한 중복 요청 방지 (SETNX)
- 인증 정보 context.Items에 저장

## RedisMemoryDb / InMemoryDb
- 인증 토큰과 사용자 정보 관리
- Redis 혹은 테스트용 In-Memory 선택 가능


## 🚕 Service
### AccountService
- 계정 생성 시 기본 캐릭터, 아이템, 재화, 미션 초기화
- 로그인 시 유저 전체 상태 정보 제공 (캐릭터, 인벤토리, 재화 등)

### CharacterService
- 캐릭터/아이템 장착 및 해제
- 행동 처리 → 재화 차감 → 미션 업데이트 → 경험치 적용

### MissionService
- 미션 진행도 업데이트
- 완료된 미션에 대한 보상 지급 처리

### MasterHandler
- 마스터 데이터 조회 캐시 핸들러
- InfoItem, InfoGoods, InfoLevel 등 타입 기반 조회 제공


---
