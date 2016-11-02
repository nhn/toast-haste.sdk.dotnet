![Logo](https://cloud.githubusercontent.com/assets/4951898/19913934/252fd2d8-a0ea-11e6-92e2-c4225e97a020.png)

# TOAST Haste SDK for .NET
`TOAST Haste SDK for .NET` 는 UDP 기반의 게임 서버 프레임워크인 `TOAST Haste framework` 를 위한 닷넷용 클라이언트 SDK 입니다.

이 SDK는 Unity3D에서도 동작합니다.

[![Englsh](https://img.shields.io/badge/Language-English-red.svg)](README.md)
![Korean](https://img.shields.io/badge/Language-Korean-lightgrey.svg)

## Features
### 다양한 QoS, 그리고 멀티플렉싱
- 실시간 멀티플레이 게임에 필요한 다음과 같은 QoS를 제공한다.
    - Reliable-Sequenced, Unreliable-Sequenced, Reliable-Fragmented.
- 도메인간 간섭이 최소화된 멀티플렉싱을 제공한다.

### 암호화
- 연결이 성립될 때마다 새로운 암호화 키를 생성한다.
- 게임특성에 따라서 필요한 데이터를 선택해서 암호화 할 수 있다.

### Wi-Fi Cellular handover
- 모바일 환경에서 Wi-Fi, 셀룰러 네트워크간 전환에 따른 IP 주소 변경에도 효율적으로 대응한다.

## Prerequisites
- 빌드에 필요한 .NET Framework 버전:
    - .NET Framework 3.5 Client Profile
- Unity3D 버전:
    - Unity3D 5.3 이상

## Versioning
- TOAST Haste SDK for .NET 의 버전은 [Semantic Versioning 2.0](http://semver.org/) 을 따른다.
- 버전을 MAJOR.MINOR.PATCH 로 표현하며:
    1. 기존 버전과 호환되지 않게 API가 바뀌면 “MAJOR 버전”을 올리고,
    2. 기존 버전과 호환되면서 새로운 기능을 추가할 때는 “MINOR 버전”을 올리고,
    3. 기존 버전과 호환되면서 버그를 수정한 것이라면 “BUILD 버전”을 올린다.
    - MAJOR.MINOR.BUILD 형식에 정식배포 전 버전이나 빌드 메타데이터를 위한 라벨을 덧붙이는 방법도 있다.

## Documetation
- 문서는 [GitHub의 Wiki](https://github.com/nhnent/toast-haste.sdk.dotnet/wiki) 를 참조한다.

## Roadmap
- NHN Entertainment 에서는 Toast Cloud Real-time Multiplayer(이하 RTM) 를 TOAST Haste를 이용해서 서비스하고 있다.
- 그래서 아래 로드맵에 따라서 성능을 최적화하고 사용성을 향상시키는 노력을 꾸준히 할 예정이다. 

### Milestones

|Milestone|Release Date|
|---------|------------|
|1.0.0    |   Sept 2016|
|1.1.0    | 2017 |

### Planned 1.1.0 features
- WebGL을 위한 WebSocket 지원.

## Contributing
- Issues: GitHub의 이슈 섹션을 통해서 버그를 알려주세요
- Source Code Contributions:
    - [Contribution Guidelines for TOAST Haste](./CONTRIBUTING.md) 문서를 참조하면 됩니다.

## Mailing list
- dl_haste@nhnent.com

## Contributor
- 권오범 (Founder)
- 김태경

## License
TOAST Haste SDK for .NET is licensed under the Apache 2.0 license, see [LICENSE](LICENSE.txt) for details.
```
Copyright 2016 NHN Entertainment Corp.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

```