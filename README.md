![Logo](https://cloud.githubusercontent.com/assets/4951898/21089491/ccdd9672-c07b-11e6-81c2-466374640a25.png)
===============

## TOAST Haste SDK for .NET
`TOAST Haste SDK for .NET` is SDK for `TOAST Haste framework` that is game server framework based on UDP.

This SDK is compatible in Unity3D.

![Englsh](https://img.shields.io/badge/Language-English-lightgrey.svg) 
[![Korean](https://img.shields.io/badge/Language-Korean-blue.svg)](README_KR.md)

## Features
### Various QoS, and Multiplexing
- Provide the follow QoS that the real time multiplayer game needs.
    - Reliable-Sequenced, Unreliable-Sequenced, Reliable-Fragmented.
- Provide the multiplexing that minimizes interferences between domains.

### Cryptography
- Generate a unique key for encryption whenever the connection was established.
- Can encrypt selected data which you need according to the game characteristic.

### Wi-Fi Cellular handover
- Respond effectively to the IP address changes caused by switching between the cellular network and the Wi-Fi network in a mobile environment.

## Prerequisites
- .NET Framework version required to build SDK:
    - .NET Framework 3.5 Client Profile
- Unity3D version required to build in Unity3D:
    - Unity3D 5.3 or later.

## Versioning
- The version of TOAST Haste SDK for .NET follows [Semantic Versioning 2.0](http://semver.org/).
- Given a version number MAJOR.MINOR.PATCH, increment the:
    1. MAJOR version when you make incompatible API changes,
    2. MINOR version when you add functionality in a backwards-compatible manner, and
    3. PATCH version when you make backwards-compatible bug fixes.
    - Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.

## Documetation
- Reference to the [Wiki section of GitHub](https://github.com/nhnent/toast-haste.sdk.dotnet/wiki).

## Roadmap
- At NHN Entertainment, we service Toast Cloud Real-time Multiplayer(a.k.a. RTM) developed by TOAST Haste.
- So, We will try to improve performance and convenience according to this roadmap.

### Milestones

|Milestone|Release Date|
|---------|------------|
|1.0.0    |   Sept 2016|
|1.1.0    | 2017 |

### Planned 1.1.0 features
- Support WebSocket for WebGL.

## Contributing
- Issues: Please report bugs using the Issues section of GitHub
- Source Code Contributions:
    - Please follow the [Contribution Guidelines for TOAST Haste](./CONTRIBUTING.md).

## Mailing list
- dl_haste@nhnent.com

## Contributor
- Ethan, Kwon (Founder)
- Tae gyeong, Kim

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