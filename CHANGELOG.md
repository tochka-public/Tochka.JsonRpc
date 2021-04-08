### [1.0.25-beta.1](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.24...v1.0.25-beta.1) (2021-04-08)

### [1.0.24](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.23...v1.0.24) (2021-01-29)

### [1.0.23](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.22...v1.0.23) (2020-12-21)


### Bug Fixes

* **server:** properly name middleware ([b16ffdf](https://github.com/tochka-public/Tochka.JsonRpc/commit/b16ffdf0941bc9c3f4f63b76fbda5ed0b4a1492c))

### [1.0.22](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.21...v1.0.22) (2020-12-17)


### Bug Fixes

* **server:** also fixed tests ([967d52a](https://github.com/tochka-public/Tochka.JsonRpc/commit/967d52a1a8a3788c5f266a6980cdef53645d9f1a))
* **server:** split attributes back for better usability ([628d90d](https://github.com/tochka-public/Tochka.JsonRpc/commit/628d90d9a21b58b441d724075739a7371c3f5d7c))

### [1.0.21](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.20...v1.0.21) (2020-12-14)

### [1.0.20](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.19...v1.0.20) (2020-12-10)


### Features

* **server:** Swagger and OpenRPC generation


### [1.0.16](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.15...v1.0.16) (2020-12-03)


### Features

* **client:** allow urls to end without trailing slash ([#22](https://github.com/tochka-public/Tochka.JsonRpc/pull/22))

### [1.0.15](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.14...v1.0.15) (2020-08-20)


### Bug Fixes

* **client:** null if error data was not present ([a51c29d](https://github.com/tochka-public/Tochka.JsonRpc/commit/a51c29d3910b63786298f9e5b8b8abf7a5d6d62f))

### [1.0.14](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.13...v1.0.14) (2020-08-18)


### Features

* **server:** add sample request-response logging for reference ([ba01b6f](https://github.com/tochka-public/Tochka.JsonRpc/commit/ba01b6f7ad24674878fecd278622d42e77215bb0))
* middleware now checks routes instead of verb/content-type heuristics ([3ce2177](https://github.com/tochka-public/Tochka.JsonRpc/commit/3ce217764770fe15f4569447b231331ec7842308))
* rename rpc to jsonrpc ([5d87982](https://github.com/tochka-public/Tochka.JsonRpc/commit/5d8798222e1c449aff5da4d757a35626d29b005c))


### Bug Fixes

* **client:** proper compare ids when null ([022619c](https://github.com/tochka-public/Tochka.JsonRpc/commit/022619c53de71b6d913c7b50ea4846f91236da51))
* **client:** proper deserialization of ids ([2fbc904](https://github.com/tochka-public/Tochka.JsonRpc/commit/2fbc904f98b322211bdc82c5d20e89e0b6588616))
* **server:** logging checks for rpc controller ([51b9e7a](https://github.com/tochka-public/Tochka.JsonRpc/commit/51b9e7a3221844fb56e08ea69b405d2235528bc6))
* forgotten changes to use route checks ([dca88be](https://github.com/tochka-public/Tochka.JsonRpc/commit/dca88beb72d545b31a3d9451eeea670ddd5a679f))
* trim slash at url end for proper validation ([3d90df9](https://github.com/tochka-public/Tochka.JsonRpc/commit/3d90df930fc9d1b899c0e12f02a454b1b8a90d04))

### [1.0.12](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.11...v1.0.12) (2020-08-18)


### Bug Fixes

* **client:** proper deserialization of ids ([2fbc904](https://github.com/tochka-public/Tochka.JsonRpc/commit/2fbc904f98b322211bdc82c5d20e89e0b6588616))

### [1.0.11](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.10...v1.0.11) (2020-07-30)


### Bug Fixes

* **client:** proper compare ids when null ([022619c](https://github.com/tochka-public/Tochka.JsonRpc/commit/022619c53de71b6d913c7b50ea4846f91236da51))

### [1.0.10](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.9...v1.0.10) (2020-07-21)


### Bug Fixes

* trim slash at url end for proper validation ([3d90df9](https://github.com/tochka-public/Tochka.JsonRpc/commit/3d90df930fc9d1b899c0e12f02a454b1b8a90d04))

### [1.0.9](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.8...v1.0.9) (2020-07-21)


### Bug Fixes

* forgotten changes to use route checks ([dca88be](https://github.com/tochka-public/Tochka.JsonRpc/commit/dca88beb72d545b31a3d9451eeea670ddd5a679f))

### [1.0.8](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.7...v1.0.8) (2020-07-21)


### Features

* middleware now checks routes instead of verb/content-type heuristics ([3ce2177](https://github.com/tochka-public/Tochka.JsonRpc/commit/3ce217764770fe15f4569447b231331ec7842308))

### [1.0.7](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.6...v1.0.7) (2020-07-01)


### Features

* rename rpc to jsonrpc ([5d87982](https://github.com/tochka-public/Tochka.JsonRpc/commit/5d8798222e1c449aff5da4d757a35626d29b005c))

### [1.0.6](https://github.com/tochka-public/Tochka.JsonRpc/compare/v1.0.5...v1.0.6) (2020-06-30)


### Features

* add docs template ([41dc8c6](https://github.com/tochka-public/Tochka.JsonRpc/commit/41dc8c67a2d8235305c62fea654f98989bbb6d4a))


### Bug Fixes

* csproj properties, sourcelink support ([4f0b664](https://github.com/tochka-public/Tochka.JsonRpc/commit/4f0b664b6beaa6acc537389bd84fa6752e9aeb83))
* remove PR trigger ([f0db5fc](https://github.com/tochka-public/Tochka.JsonRpc/commit/f0db5fcacf1c4f7fd8b7ecfcbb06cfe7f8de5f73))
* rm wrong url in semantic release config ([5d31f94](https://github.com/tochka-public/Tochka.JsonRpc/commit/5d31f94929f181b9fbdea7d46e8db4c69e74aaf4))

### [1.0.5](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/compare/v1.0.4...v1.0.5) (2020-06-30)


### Bug Fixes

* polishing ci/cd scripts ([2966fe0](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/2966fe0a623cc5c6dde1d7b221e14e3427e2133d))

### [1.0.4](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/compare/v1.0.3...v1.0.4) (2020-06-30)


### Bug Fixes

* polishing ci/cd scripts ([c6c1261](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/c6c1261da4844c5f583edecee466cad0a878ac58))

### [1.0.3](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/compare/v1.0.2...v1.0.3) (2020-06-30)


### Bug Fixes

* polishing ci/cd scripts ([66ae1ce](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/66ae1ce7cb927a23db2f67dd7344f515aa26f631))
* polishing ci/cd scripts ([98b2a19](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/98b2a19071d75df2e300b6af9d3d1384651d46c1))
* polishing ci/cd scripts ([0570796](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/057079632da505396421ccdbf447c3246671fa1f))

### [1.0.2](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/compare/v1.0.1...v1.0.2) (2020-06-30)


### Bug Fixes

* polishing ci/cd scripts ([c3636ea](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/c3636ea57e4ffa6cbc0611a762ff11b4d6579733))

### [1.0.1](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/compare/v1.0.0...v1.0.1) (2020-06-30)


### Bug Fixes

* polishing ci/cd scripts ([2dbe8af](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/2dbe8af39b8ca9e8649b8df4153dafbd4a9fec81))
* polishing ci/cd scripts ([64648f6](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/64648f6a8a594ecdcabc9478bbf9cbd9fc3d998d))

## 1.0.0 (2020-06-30)


### Features

* initial configuration for semantic-release ([b6f23a6](https://gitlab.tochka-tech.com/tochka-public/Tochka.JsonRpc/commit/b6f23a6c4e8c7c1a2a1286812a55b2fa81b08e1a))
