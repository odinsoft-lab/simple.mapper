# Simple.AutoMapper Roadmap

본 문서는 중·장기적으로 계획된 개선 과제들을 정리합니다. 우선순위와 범위는 프로젝트 상황에 따라 변동될 수 있습니다.

마지막 업데이트: 2025-08-21

## 목표 축
- 기능 완성도: 구성(ForMember 등)과 매핑 시나리오 커버리지 확장
- 성능/안정성: 저부하·고부하 모두에서 예측 가능한 성능과 낮은 GC 압력
- 사용성/DX: 명확한 API, 문서, 진단/검증 도구, IDE 친화성
- 통합: .NET DI, EF Core, CI/CD, NuGet 품질 메타데이터

## 기능 로드맵 (중기 1~3개월)
- 구성 API 고도화
  - ForMember 구현: MapFrom(표현식), Constant, Condition, NullSubstitute
  - Ignore는 현행 유지, 규칙 기반 Ignore/Include 옵션 추가
  - ConstructUsing, BeforeMap/AfterMap 훅
  - ReverseMap 지원(양방향 구성 생성)
- 컬렉션/타입 커버리지 확대
  - 배열/HashSet/Dictionary 매핑
  - record, init-only, 읽기 전용 컬렉션 대응 전략
  - 다형성 매핑(인터페이스/기반형 → 파생형) 전략 옵션
- 검증/진단
  - 구성 검증 API(누락된 멤버, 타입 불일치, 불가능한 변환 탐지)
  - 런타임 진단 로그 레벨/이벤트 소스

## 성능 로드맵 (중기 1~3개월)
- BenchmarkDotNet 기반 마이크로벤치 스위트 및 기준선 수립
- 캐시 고도화: 사전 워밍업, 용량/만료 정책, 통계 지표 추가
- 할당 감소: MapList의 스트리밍 옵션(IReadOnlyList/Span 입력 최적화 대안 설계)

## 통합/플랫폼 로드맵 (중기 1~3개월)
- Microsoft.Extensions.DependencyInjection 통합(싱글턴 엔진, 프로필 스캔)
- XML Doc 주석 정비 및 NuGet에 포함, SourceLink/심볼 패키지
- GitHub Actions: 빌드/테스트/패키징/릴리스 드래프트 자동화

## 장기 로드맵 (3~6개월+)
- IQueryable Projection 지원(EF Core 번역 가능한 식 트리 생성)
- C# 소스 생성기(Source Generator)로 정적 매퍼 생성(런타임 폴백 유지)
- 규칙 기반 네이밍/플래트닝 규칙(예: `Parent.ChildId` ↔ `ParentChildId`)
- 순환 참조 처리/객체 동일성 보존 옵션/최대 깊이 제한
- 정적 분석기(Analyzer) 제공: 미매핑 멤버, 위험한 변환 감지
- 웹 문서 사이트(DocFX/Docusaurus)와 예제 갤러리 확장

## 품질 게이트/정책
- SemVer 엄수, 주요 API 변경 시 마이그레이션 가이드 제공
- 코드 커버리지 목표(예: ≥ 85%)와 성능 예산 유지
- 멀티 타깃(netstandard2.0/2.1, net8/9) 호환 지속 점검

## 브레이킹 체인지 관리
- MappingEngine.Map → MapItem 전환 완료. 후속 릴리스에서 릴리스 노트/마이그레이션 가이드 고도화
- 향후 대규모 기능 추가 시 실험적 플래그/프리릴리스 채널 사용 검토
