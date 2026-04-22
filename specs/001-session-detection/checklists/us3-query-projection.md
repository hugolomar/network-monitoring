# US3 query projection — Requirements quality checklist

**Purpose**: “Unit tests for English” for **User Story 3** y **FR-017–FR-021**, **SC-006**, edge cases de consulta, alineación **spec ↔ plan ↔ ADR 0009**, y **FR-018** frente a US1/US2. No sustituye pruebas de sistema.

**Created**: 2026-04-20  
**Revised**: 2026-04-22  
**Feature**: `/home/hugo/network-monitoring/specs/001-session-detection/spec.md` (US3, FR-017–FR-021, SC-006; **ADR 0009** en `docs/adr/0009-elasticsearch-for-session-detection-query.md`)

**Note**: Cada ítem pregunta si el **texto** del requisito es completo, claro, medible o consistente — **no** si Elasticsearch, Connect o el probe “ya están implementados y pasan tests”.

---

## Completitud (US3)

- [x] CHK001 ¿Están los filtros mínimos de la capacidad de consulta (rango temporal + dimensiones alineadas con **FR-006**) enlazados de forma que un revisor pueda comprobar cobertura sin dispersar criterios entre secciones? [Completeness, FR-017, FR-006]
- [x] CHK002 ¿Queda definido qué cuenta como **“capacidad de consulta documentada (nombre específico de producto/despliegue)”** a nivel de especificación, o el lector debe inferirlo solo desde **plan.md** / artefactos de implementación? [Gap, Clarity, FR-017]
- [x] CHK003 ¿La dependencia de US3 respecto a publicación en el stream (**US2**) aparece como prerequisito explícito en un lugar citables (**Assumptions** / US3), sin ambigüedad de que “histórico” implique otra fuente que no sea eventos ya publicados? [Completeness, Assumption, US3, Assumptions (spec)]
- [x] CHK004 ¿**FR-021** delimita con claridad qué queda en política organizacional vs qué debe poder verificarse en criterios de éxito (**SC-006**) sin exigir un diseño RBAC completo en el spec? [Clarity, FR-021, SC-006]

## Claridad y medibilidad

- [x] CHK005 ¿**FR-019** (“conjunto acotado”, paginación/cursores/límites) es revisable sin depender de tecnología concreta, y las **excepciones operativas documentadas** están acotadas para evitar que “bounded” sea vacío de significado? [Clarity, Ambiguity, FR-019]
- [x] CHK006 ¿**FR-020** obliga a documentar **o bien** demora máxima aceptable **o bien** consistencia eventual de modo que no queden ambas lecturas opcionas sin criterio? [Clarity, FR-020]
- [x] CHK007 ¿**SC-006** define o remite a una regla explícita de **muestreo** (“sampled returned”) para que el 100% de coincidencia con el contrato sea objetivamente aplicable, o ese método solo vive fuera del spec? [Measurability, Gap, SC-006]

## Consistencia con US1 / US2 y contrato único (FR-018)

- [x] CHK008 ¿**FR-018** ancla la semántica de consulta al **mismo** contrato versionado que **FR-015** (`contracts/`) sin definiciones paralelas de “sesión” en el cuerpo del spec? [Consistency, FR-018, FR-015, Key Entities]
- [x] CHK009 ¿Los escenarios de aceptación de **US3** son compatibles con que solo existan detecciones que ya cumplen el contrato en publicación (sin campos o significados no presentes en stream/console)? [Consistency, US3 Acceptance, FR-018]
- [x] CHK010 ¿La regla de identidad **FR-005** (sin clave sustituta del probe) está reflejada de forma coherente en cómo US3 habla de **campos obligatorios** y resultados de consulta? [Consistency, FR-005, FR-018, US3 Independent Test]
- [x] CHK011 ¿Las reglas de supresión de duplicados en emisión (**FR-011**) y “un evento por detección validada” (**FR-013**) tienen relación explícita en el texto con duplicados/overlap en **resultados de consulta** (Edge Cases US3)? [Consistency, Gap, FR-011, FR-013, Edge Cases (US3)]

## Edge cases (consultas)

- [x] CHK012 ¿El resultado **vacío** como caso normal (no fallo) está en escenarios de aceptación y alineado con la narrativa de operadores? [Coverage, US3 scenario 3, FR-017]
- [x] CHK013 ¿Los criterios muy amplios y conjuntos grandes enlazan **Edge Cases (US3)** con **FR-019** de forma redundante pero consistente? [Consistency, Edge Cases (US3), FR-019]
- [x] CHK014 ¿El retraso emisión → disponibilidad para consulta enlaza **Edge Cases (US3)** con la obligación de documentación **FR-020**? [Coverage, Edge Cases (US3), FR-020]
- [x] CHK015 ¿Las filas duplicadas o solapadas exigen documentación de deduplicación/consolidación **para consumidores de la capacidad de consulta** sin contradecir **FR-018**? [Completeness, Edge Cases (US3), FR-018]

## Spec vs plan / ADR 0009 (sin fuga indebida de implementación)

- [x] CHK016 ¿El **spec** permanece sin nombres de producto de infraestructura donde no son necesarios para el “qué”, delegando Elasticsearch/Connect a **ADR 0009** y **plan.md** según la intención del feature? [Consistency, FR-017–FR-021, ADR 0009, plan.md §US3]
- [x] CHK017 ¿La separación **log durable (Kafka) vs proyección de lectura** y la **consistencia eventual** (**FR-020**) están alineadas entre spec y **ADR 0009 Consequences** sin duplicar detalle de conectores en el spec? [Consistency, FR-020, ADR 0009]
- [x] CHK018 ¿Expectativas de **TLS/autenticación** para la superficie de consulta quedan trazables entre **FR-016** (stream), **FR-021** (acceso), y lo que **plan.md** añade para Elasticsearch, sin contradicción silenciosa? [Consistency, Gap, FR-016, FR-021, plan.md Constitution/US3]

## Trazabilidad a verificación

- [x] CHK019 ¿El **Independent Test** de US3 y **SC-006** remiten de forma explícita al **mismo artefacto de contrato** que **SC-005** / **FR-015**, de modo que la verificación de consulta no dependa de una definición de “contrato” distinta? [Traceability, US3 Independent Test, SC-006, SC-005, FR-015]

---

## Notes

- Marca `[x]` solo cuando, **revisando el texto** de `spec.md` / `plan.md` / ADR, la pregunta queda respondida. Eso no depende de que T051–T061 estén hechos en código.
- **Revisión completada (2026-04-22):** Los 19 ítems se consideran satisfechos. **CHK002:** el *nombre* de producto de referencia (p. ej. Elasticsearch) consta en **ADR 0009** y `plan.md` §US3; el spec fija en **FR-017** la obligación de una capacidad con nombre *documentado* (producto/despliegue). **CHK011:** se añadió en `spec.md` (Edge Cases US3) la referencia explícita a **FR-011/FR-013** frente a solapamientos en la **superficie de consulta** y **FR-018** (líneas actualizadas junto a este checklist).
- La **implementación** (Elasticsearch, Connect, scripts) se gobierna con **`tasks.md`**. Cuando US3 esté desplegado, **SC-006** y `research.md` prueban **comportamiento**; este checklist no las sustituye.
- Reabrir o actualizar **Revised** si el spec cambia de forma material.
