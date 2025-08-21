# Simple.AutoMapper v1.0.5 — Release

Date: 2025-08-21

## Summary
Simple.AutoMapper는 .NET용 고성능 매핑 라이브러리입니다. 공개 API는 간단한 리플렉션 기반 Mapper 중심이며, 내부적으로는 컴파일된 MappingEngine을 사용해 첫 호출 시 컴파일·캐시하여 이후 호출 성능을 높입니다. 이번 릴리스는 안정성, 멀티 타겟팅, 문서 정리에 초점을 맞췄습니다.

Targets: netstandard2.0, netstandard2.1, net8.0, net9.0

## Highlights
- 간결한 공개 API와 내부 컴파일 엔진의 통합 동작
- 컬렉션 매핑 및 리스트 동기화 헬퍼(SyncResult)
- 스레드 안전 캐싱, 결정적 빌드, 임베디드 PDB 및 심볼 패키지(snupkg)
- ReverseMap 지원: 양방향 매핑 구성 자동 생성 ✅
- PreserveReferences / MaxDepth 옵션 추가(실험적) — 순환 참조/최대 깊이 제어는 향후 보강 예정
- 문서 정비(README, ROADMAP) 및 샘플 갱신: in-place update 예제 추가

## Breaking/Behavioral Notes
- ForMember 구성은 현재 캡처만 하며 컴파일된 매핑에는 아직 반영되지 않습니다.
- 순환 참조 처리는 v1.0.5에서는 완전하지 않습니다. PreserveReferences/MaxDepth는 실험적 단계이며, 특정 그래프에서 기대대로 동작하지 않을 수 있습니다.
- 공개 API의 시그니처 변경은 없습니다(1.0.x).

## API Surface
- Mapper(공개, 리플렉션 기반)
  - `Map<TSource, TDestination>(TSource)`
  - `Map<TDestination>(object source)`
  - `Map<TSource, TDestination>(IEnumerable<TSource>)`
  - `Map<TSource, TDestination>(TSource source, TDestination destination)` — in-place 업데이트
  - 리스트 동기화 헬퍼: upsert/remove 결과를 `SyncResult`로 반환
- MappingEngine(내부, 컴파일 기반)
  - `CreateMap<TSource, TDestination>()` 구성 API(향후 Ignore/ForMember 적용 고도화)

## Migration and Usage
일반 사용은 공개 Mapper API를 권장합니다. 구성형 시나리오는 `CreateMap`으로 한 번 정의 후 공개 API로 매핑을 호출합니다.

```csharp
// 구성 예시
Mapper.CreateMap<User, UserDto>();
Mapper.CreateMap<Address, AddressDto>();

// 단건/컬렉션 매핑
var dto = Mapper.Map<User, UserDto>(user);
var dtos = Mapper.Map<User, UserDto>(users);

// in-place 업데이트
Mapper.Map(source, existingDestination);
```

EF Core에서는 먼저 materialize한 뒤 매핑하세요.

```csharp
var users = await db.Users.AsNoTracking().Include(u => u.Address).ToListAsync();
var dtos = Mapper.Map<User, UserDto>(users);
```

### ReverseMap
```csharp
Mapper.CreateMap<User, UserDto>()
  .ReverseMap();
```

### PreserveReferences / MaxDepth (실험적)
```csharp
Mapper.CreateMap<Entity, EntityDto>()
  .PreserveReferences()  // 순환 참조 추적
  .MaxDepth(5);          // 최대 깊이 제한
```
주의: v1.0.5에서는 깊이/참조 추적이 제한적일 수 있습니다. 심층 그래프에서는 보장되지 않습니다.

## Performance Notes
- 첫 호출 시 컴파일 및 캐시가 이루어지며, 이후 동일 타입 쌍 매핑은 빠르게 동작합니다.
- 실제 성능은 환경에 따라 달라지며, 첫 매핑은 워밍업으로 간주하세요.

## Known Limitations
- ForMember 규칙은 저장되지만 아직 컴파일된 식에 반영되지 않습니다(향후 적용 예정). Ignore는 동작합니다.
- 순환 참조는 v1.0.5에서 완전 지원되지 않습니다(스택 오버플로 위험). PreserveReferences/MaxDepth는 실험적입니다.
- 대상 타입은 매개변수 없는 생성자(new())가 필요합니다.
- IQueryable 내부에서 매핑 API를 호출하지 마세요. SQL로 번역되지 않습니다.

## Next
- BenchmarkDotNet 기반 벤치마크 프로젝트 추가 및 기준 수립
- ForMember(MapFrom/Condition 등) 컴파일 반영 완성
- DI 연동 및 프로필 시스템 정리, 진단/검증 API 추가

## Credits
Built by ODINSOFT. See README for team.
