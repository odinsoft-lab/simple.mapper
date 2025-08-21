# Release Draft

Date: 2025-08-21

## Highlights
- API 정리: MappingEngine.Map 제거, MapItem을 단일 권장 API로 확정
- 문서 업데이트: README/DEPLOYMENT/CLAUDE 및 신규 ROADMAP 추가
- 테스트 안정화: 플래키 성능 테스트 개선(고해상도 측정 및 허용치 조정)

## Breaking Changes
- MappingEngine.Map<TSource, TDestination>(TSource) 제거
  - 대체: MappingEngine.MapItem<TSource, TDestination>(TSource)
  - 영향: 엔진 기반 호출부는 모두 MapItem으로 변경 필요(샘플/테스트/README 반영 완료)

## Improvements
- Expression 기반 매핑: 중첩 객체/컬렉션에 대해 MapItem/MapList를 재사용하도록 유지
- 문서 품질: 프로젝트 목표와 계획을 ROADMAP.md로 가시화

## Migration Guide
- 기존 코드 예시
  ```csharp
  var engine = new MappingEngine();
  engine.CreateMap<Foo, Bar>();
  // old
  // var bar = engine.Map<Foo, Bar>(foo);
  // new
  var bar = engine.MapItem<Foo, Bar>(foo);
  ```

## Known Issues
- 성능 테스트는 환경에 따라 변동이 존재. 공식 벤치마크는 차기 릴리스에서 제공 예정(BenchmarkDotNet).

## Next
- BenchmarkDotNet 벤치 프로젝트 추가 및 기준선 수립
- ForMember(MapFrom/Condition 등) 구현 시작, 검사/진단 API 설계
