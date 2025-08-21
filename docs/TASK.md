# Tasks (Short-term / Immediate)

Updated: 2025-08-21

## Must-do (이번 스프린트)
- [ ] 릴리스 노트 작성: MappingEngine.Map 제거 및 MapItem 전환 공지, README 반영 확인
- [ ] 성능 테스트 분리: BenchmarkDotNet 프로젝트 신설(tests/Benchmarks) 및 기존 xUnit 성능 테스트 이관/간소화
- [ ] 문서 보강: ForMember 제한 사항과 ROADMAP 링크 추가, DEPLOYMENT 최신화 재확인
- [ ] CI 구성: GitHub Actions 워크플로 초안(push/PR 시 빌드+테스트)

## Quick wins(당장 적용 가능)
- [ ] MappingEngine/Mapper 공용 XML 주석 보강 및 NuGet 패키지 포함 설정
- [ ] Sample 코드 최신화 재확인(MapItem 사용, net9 빌드 확인)
- [ ] 성능 테스트 허용치/불안정성 주석 추가(테스트 유지 사유와 대안 문서화)

## Backlog(단기)
- [ ] DI 통합 초안: IServiceCollection 확장 메서드 설계(엔진 싱글턴 등록)
- [ ] MapList의 null → 빈 리스트로 반환 옵션 검토(옵션 플래그)
- [ ] 컬렉션 타입 추가 테스트(배열/HashSet/Dictionary) 스켈레톤 생성
