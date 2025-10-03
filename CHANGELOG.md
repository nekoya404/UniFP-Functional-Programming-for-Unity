# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-02

### Added
- ✨ Result 모나드 구현
  - Success/Failure 상태 표현
  - 구조체 기반 Zero Allocation 설계
  - IsSuccess, IsFailure 속성
  - Value, Error 접근자

- 🔗 Pipe 유틸리티
  - Start: 파이프라인 시작
  - Bind: 모나드 체이닝
  - Map: 값 변환
  - Do: 부수 효과 (디버깅/로깅)
  - DoOnError: 에러 시 부수 효과
  - Filter: 조건 검증
  - Recover: 에러 복구
  - Try/TryBind: 예외 처리

- 📝 Result 헬퍼 메서드
  - Match: 패턴 매칭
  - OnSuccess/OnFailure: 체이닝 가능한 액션
  - GetValueOrDefault: 기본값 반환
  - GetValueOrElse: 함수로 대체값 생성

- 📚 문서
  - README.md: 프로젝트 소개 및 사용법
  - 예제 코드: Test.cs
  - API 주석: 모든 public 메서드에 XML 문서 주석

- 🎯 Unity 통합
  - Assembly Definition 파일
  - Meta 파일
  - Package.json

### Design Decisions
- 구조체 사용: GC 부담 최소화
- 널 체크 최소화: 성능과 가독성 향상
- 명시적 에러 처리: 예외 대신 Result 사용
- 불변성: 모든 연산은 새 Result 반환
- VContainer 호환: DI 패턴과 함께 사용 가능

[1.0.0]: https://github.com/YOUR_USERNAME/UniFP/releases/tag/v1.0.0
