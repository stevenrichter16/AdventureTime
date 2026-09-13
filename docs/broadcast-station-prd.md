# Broadcast Station — Product Requirements

## Summary

The Broadcast Station changes how an episode enters the app: it must first be **announced** with a manifest, the announcement puts the station **on the air**, and only a live, prepared session can **receive** the episode when it arrives. The payoff is analysis that knows history ("third episode in a row where Finn doubts himself") rather than analysis that starts cold, and a station whose state is always answerable: what is on the air, what is expected, what went wrong. It is for you (developer and, day to day, operator), the scripts that announce and deliver episodes, and the viewer who reads the results. This is the smallest system that is still unmistakably the station, and it says where to stop.

## Goals and non-goals

**Goals**

- **Gate.** Nothing is stored as an episode unless it was announced and a live session received it.
- **Warm.** A session prepares while waiting, and that preparation demonstrably reaches the analysis as continuity notes.
- **Match.** Content is checked against its manifest by fixed rules; every discrepancy is listed.
- **Finite.** On-air slots are limited and configurable; unfulfilled sessions expire; nothing holds a slot forever.
- **Durable.** A restart loses nothing. The station never silently forgets a promise.
- **Visible.** No failure is silent: every refusal, expiry, and failure has a recorded reason.

**Non-goals**

These are deliberate. If you find yourself building one, stop.

- No process-on-arrival fallback or bypass flag while the station is running, even for admins. The single exception is the grandfather import (FR-15), which is unavailable while the app serves requests.
- No waiting list when slots are full; the manifest is refused.
- No editing a manifest in place; cancel and re-announce.
- No hold-for-review workflow; flags are informational.
- No change to the existing EpisodeAnalysis fields or to the provider. Continuity notes are an addition attached to the analysis, not a reshaping of it.
- No re-analysis of the 21 existing episodes; they are grandfathered and serve as history.
- No authentication, notifications, or live push.

## Who uses it

| Actor | Who they are today | What they want |
|---|---|---|
| **Announcer** | A person announcing by hand, or a script feeding fan-written episodes | A clear yes/no with a deadline, never announcing into a void |
| **Deliverer** | Same person or script, later, sending full content | A receipt: what matched, that processing started, or exactly why not |
| **Operator** | You, watching the app | One board: on air, expected, failed and why; cancel, retry, abandon; nothing dies without a trace |
| **Viewer user** | Anyone in the React viewer | Analyses as before, plus what is coming and continuity insight; nothing partial shown as finished |
| **The Station** (system) | New | Hold slots, prepare, watch the clock, match, process, recover |

## Concepts

### Episode Manifest ("the programming schedule")

An announcement that an episode is coming. It carries identity and description, never content: the schedule never carries the broadcast. Season plus episode number is its identity, and it is the only way to open the door.

| Field | Rule |
|---|---|
| Season, episode number | Required; together, the manifest's identity |
| Title | Required |
| Major characters | Required; at least one |
| Processing kind | Required: Standard, Character-focused, or Relationship-focused |
| Focus character | Optional; required for Character-focused; must be among the announced cast |
| Minor characters, locations, runtime, production code, air date | Optional |
| Synopsis, plot, transcript, dialogue line count | Forbidden; their presence refuses the manifest |

Processing kind changes emphasis only, never output shape. **Standard** weighs all analysis sections evenly. **Character-focused** leans into the focus character's moods and arc. **Relationship-focused** leans into dynamics among the announced cast.

| Manifest state | Meaning |
|---|---|
| Announced | Accepted; the station is opening its session. Momentary. |
| On Air | Its session holds a slot and can receive the episode. |
| Fulfilled | Its episode was accepted. |
| Expired | Deadline passed with no accepted episode. |
| Cancelled | Withdrawn before an episode was accepted, or abandoned after a failure. |
| Refused | Never accepted; recorded with a reason. Not a live state. |

### Session ("the station on the air")

The station live for one manifest. Opening it consumes one on-air slot and starts preparation: loading each announced character's prior moods, relationships, and arcs from stored analyses, and choosing emphasis from the processing kind and focus character. The session owns the clock until an episode is accepted; after that, the deadline no longer applies.

**History** means analyzed, stored episodes that precede the announced one in (season, episode number) order. An already-stored later-numbered episode is not history for an earlier one. "In a row" means consecutive among that character's appearances in the same order.

| Session state | Meaning | Holds a slot? |
|---|---|---|
| Preparing | Loading history, on announcement or on retry. Deliveries refused "not ready." | Yes |
| Ready | Waiting for the episode. | Yes |
| Receiving | Episode arrived; being matched. | Yes |
| Processing | Episode accepted; analysis running. | Yes |
| Completed | Analysis stored. | No |
| Expired | Deadline passed with no accepted episode. | No |
| Failed | Processing failed or timed out; episode retained; retry or abandon. | No |
| Cancelled | Closed before an episode was accepted, or abandoned by the operator after failure. | No |

Paths: Announced → Preparing → Ready → Receiving → Processing → Completed. Rejected: Receiving → Ready. Deadline or cancel: Preparing or Ready → Expired or Cancelled. Retry: Failed → Preparing → Processing. Abandon: Failed → Cancelled.

### Episode ("the broadcast")

The full content, stored exactly as today, with today's validation rules unchanged. What changes is entry: it is delivered to a session, matched against that session's manifest, and dropped in, flagged, or rejected. Once stored it links to its manifest and session.

### Match verdict

**Match**: stored and processed. **Flagged**: stored and processed; every discrepancy shown. **Rejected**: nothing stored; the session stays Ready.

### Continuity notes

Observations possible only because the station knew the history before the episode landed. Each names the characters and prior episodes it refers to. Notes are attached to the episode's analysis and appear wherever that analysis is fetched.

## User stories

**Announcer**

- **US-1** As an announcer, I want to announce an episode with its cast, setting, and processing kind, so the station prepares before content arrives.
- **US-2** As an announcer, I want an immediate answer (on air with a deadline, or refused with a reason), so I know whether to deliver.
- **US-3** As an announcer, I want to cancel a manifest, so a mistake does not hold a slot until expiry.

**Deliverer**

- **US-4** As a deliverer, I want to learn that my episode matched and processing started, or every reason it was refused, so I fix the right thing in one pass.
- **US-5** As a deliverer, I want small discrepancies flagged rather than fatal, so a missing minor character does not block the episode.

**Operator**

- **US-6** As an operator, I want one board showing what is on air, expected, done, expired, and failed, with reasons, so I never guess.
- **US-7** As an operator, I want to retry a failed session without re-delivering content, so a provider hiccup does not cost me the episode.
- **US-8** As an operator, I want to abandon a session that keeps failing, so a hopeless episode does not block its season and number forever.

**Viewer user**

- **US-9** As a viewer user, I want upcoming episodes in the season list and continuity notes on episode and character pages, so I get the "third time in a row" insight.

## Functional requirements

MUST ships in v1; SHOULD ships unless it threatens the date; COULD is v2.

### Announcing

- **FR-1 (MUST)** A manifest follows the field table: a missing required field, any forbidden field, or a focus character outside the announced cast is refused naming the field.
- **FR-2 (MUST)** A manifest for a stored episode is refused "episode already exists"; one for a season/episode already On Air is refused naming that manifest. Expired or Cancelled season/episodes may be announced again; wherever a season/episode pair identifies a manifest, it means the most recent manifest for that pair.
- **FR-3 (MUST)** With no free slot, a manifest is refused "station full," with the slot count and the earliest deadline among sessions still awaiting delivery; if none is awaiting delivery, the refusal says so and how many sessions are processing.
- **FR-4 (MUST)** Once the announcer has been told the manifest is On Air, with its identifier, session state, and deadline, the station honors that answer even if it restarts immediately afterward.
- **FR-5 (MUST)** A manifest whose session is Preparing or Ready can be cancelled, releasing the slot immediately; cancellation is refused from Receiving onward (for Failed, see FR-28).
- **FR-6 (COULD)** Amend non-identity fields before delivery. In v1, cancel and re-announce.

### Going on air / preparation

- **FR-7 (MUST)** Accepting a manifest claims one slot and opens a session in Preparing. Two announcements competing for the last slot never both succeed, and two concurrent announcements for the same season/episode never both succeed.
- **FR-8 (MUST)** While Preparing, the session loads every announced character's history (as defined under Session, grandfathered included) and records a summary naming the prior episodes used and the emphasis chosen. No history is "no prior history," not an error.
- **FR-9 (MUST)** The session becomes Ready only after preparation completes. If preparation errors, it still becomes Ready, marked "cold," with the error recorded; preparation never wedges a slot.

### Receiving an episode

- **FR-10 (MUST)** An episode enters only by being delivered against a manifest. Any other attempt to create one while the station is running stores nothing and answers "announce first."
- **FR-11 (MUST)** A delivery identifies the manifest it fulfils, by identifier or by season/episode; an identification that is ambiguous or points at two different manifests is rejected saying so.
- **FR-12 (MUST)** A delivery whose manifest is not Ready is refused with the actual outcome: "not announced," "not ready" (Preparing; retry shortly), "expired," "cancelled," or "already received" (Receiving onward; for Failed, it says the operator can retry). The session is unchanged.
- **FR-13 (MUST)** A delivery during Preparing is refused "not ready," nothing is held or stored, and the session's deadline is unchanged.
- **FR-14 (MUST)** The deliverer learns the verdict, which manifest it was judged against, every discrepancy found (not just the first), and whether processing has started.
- **FR-15 (MUST)** The grandfather import is the only way content bypasses the gate. It runs only when the station is not serving requests (seeding a database before the app is up) and is unreachable at runtime. Everything it stores is marked grandfathered.

### Matching and validation

- **FR-16 (MUST)** Content season and episode number must equal the manifest's; a mismatch is Rejected naming both. An empty transcript is Rejected "unprocessable." Today's episode validation failures are Rejected reasons, listed alongside manifest mismatches.
- **FR-17 (MUST)** Every announced major character must appear in the content's major or minor list; any missing one is Rejected naming each. Matching is case-insensitive and trimmed.
- **FR-18 (MUST)** A content major character absent from the manifest is Flagged, one flag per character.
- **FR-19 (SHOULD)** Flagged, never Rejected: a missing promised minor character or location; a title that differs case-insensitively and trimmed; a runtime off by more than a configurable tolerance (default 50 percent of the manifest's runtime); a content focus character that differs from the manifest's.
- **FR-20 (MUST)** Rejected leaves the session Ready with its deadline unchanged. Match or Flagged stores the episode, records all flags, and moves the session to Processing with no further request.

### Processing

- **FR-21 (MUST)** Processing uses the session's prepared history and emphasis. The stored result records which prior episodes and which emphasis informed it, and that record equals the preparation summary; a cold session's result records "no history."
- **FR-22 (MUST)** Processing produces continuity notes attached to the analysis, each naming the characters and prior episodes it refers to (possibly none). A cold session's notes are labeled "produced without history." The content's cast is the truth for analysis; a difference from the manifest's cast is itself a note.
- **FR-23 (MUST)** Analysis and notes are stored all-or-nothing; a session is never Completed with a partial analysis. On success it becomes Completed and releases its slot.
- **FR-24 (MUST)** If the provider fails, the session becomes Failed with the provider's reason, releases its slot, and retains the episode.
- **FR-25 (MUST)** An operator can retry a Failed session without redelivering. Retry moves it Failed → Preparing → Processing, occupies a slot, is refused "station full" when none is free, has no delivery deadline, and never leaves two analyses for one episode.
- **FR-26 (MUST)** Processing has a configurable maximum duration (default 10 minutes); past it the session becomes Failed "processing timed out," releases its slot, and is retryable.
- **FR-27 (MUST)** Analysis of a station-admitted episode starts only through its session, on admission or on retry; any other request to analyze it is refused pointing at the session. Grandfathered episodes keep today's analyze-on-request ability.
- **FR-28 (MUST)** An operator can abandon a Failed session: it becomes Cancelled "abandoned by operator," the stored episode is discarded, and the season/episode can be announced again.

### Expiry and capacity

- **FR-29 (MUST)** The slot count is configurable (default 3) and read at startup. Reducing it does not evict live sessions; no new session opens until usage drops below the new limit.
- **FR-30 (MUST)** Each session's deadline is set at acceptance from a configurable time-to-live (default 30 minutes), never reset by a restart, and applies only until an episode is accepted.
- **FR-31 (MUST)** A Preparing or Ready session with no accepted episode and a passed deadline becomes Expired within one minute, slot released, reason "no episode delivered by deadline," enforced by the station itself, not only when someone queries. Sessions that have accepted an episode, including a retry in Preparing, never expire.
- **FR-32 (MUST)** Refusals and rejections are retained for a configurable period (default 30 days); manifests and sessions are retained indefinitely. "Recent" means within that period.

### Visibility

- **FR-33 (MUST)** The operator can retrieve the board: live sessions with manifest, processing kind, time on air, deadline, cold marker, and flags; slots used versus total; recent terminal sessions, refusals, and rejections with reasons and timestamps.
- **FR-34 (MUST)** A manifest can be looked up by identifier, or by season/episode (all manifests for the pair, newest first), with its full transition history, reasons, and timestamps. A stored episode exposes its manifest, session timeline, verdict, flags, cold marker, and the prior episodes its notes cite. Grandfathered episodes are marked.

### Recovery

- **FR-35 (MUST)** Manifests, sessions, states, deadlines, flags, and preparation summaries are persisted; after a restart the board shows the same sessions as before.
- **FR-36 (MUST)** After a restart, Preparing sessions prepare again; Ready sessions keep their original deadline; sessions whose deadline passed during downtime expire on startup with reason "expired during downtime."
- **FR-37 (MUST)** A session that was Processing at restart becomes Failed with reason "interrupted by restart," episode retained, retryable per FR-25.
- **FR-38 (MUST)** At restart, an Announced session continues into Preparing. A Receiving session reverts to Ready with the unstored delivery discarded and a note "delivery discarded by restart," so the deliverer, who got no receipt, can redeliver.

## Edge cases and failure behavior

| Situation | Expected behavior |
|---|---|
| Episode arrives with no manifest | Refused "not announced"; nothing stored; rejection recorded. |
| Episode arrives while session is Preparing | Refused "not ready"; nothing held; deliverer retries shortly. |
| Manifest never fulfilled | Expired within a minute of deadline; slot released; later delivery refused "expired." |
| Content contradicts manifest: season/episode | Rejected naming both; session stays Ready. |
| Content contradicts manifest: announced major character missing | Rejected naming each; session stays Ready. |
| Content contradicts manifest: extra major, missing minor or location, title, runtime, focus | Flagged; stored and processed; flags visible. |
| Content fails today's episode validation | Rejected, every reason listed with any mismatches; session stays Ready. |
| Duplicate manifest, or manifest for an existing episode | Refused, naming the conflicting manifest or episode. |
| Duplicate episode delivery | Refused "already received"; stored episode untouched. |
| All slots busy, including a race for the last slot | Refused "station full" with earliest awaiting-delivery deadline (or "none awaiting; N processing"); exactly one racer succeeds. |
| Provider fails or hangs mid-processing | Failed with provider reason or "processing timed out"; episode retained; slot released; operator retry or abandon. |
| Analysis requested for a station episode outside its session | Refused, pointing at the session; stored analysis untouched. |
| App restarts while sessions are live | Announced continues; Preparing re-prepares; Ready keeps its deadline; Receiving reverts to Ready with a note; Processing becomes Failed "interrupted by restart"; overdue sessions expire on startup. |
| Manifest cancelled after announcement | Allowed while Preparing or Ready: slot freed; later delivery refused "cancelled." Refused from Receiving onward; Failed uses abandon. |
| Manifest edited after announcement | Not supported in v1; cancel and re-announce. |

## Acceptance criteria

Ships when all pass end to end against a fresh database seeded with the 21 episodes through the grandfather import, using a stand-in provider that returns a fixed analysis plus one continuity note naming the first prior episode in the history it received. A manual run against the real provider is expected but not a gate.

1. Delivering with no manifest is refused; nothing stored; the refusal is in the operator's list. Creating an episode any other way is refused "announce first."
2. Announcing S11E21 with Finn and Jake returns On Air with a deadline; delivering matching content stores it, processes without a second request, and yields an analysis whose recorded history and continuity note cite an earlier Season 11 episode.
3. Delivering content that omits Jake is rejected naming Jake; the session stays Ready; corrected redelivery succeeds. Delivering with unannounced BMO succeeds with a flag for BMO.
4. Delivering twice: the second is refused "already received"; exactly one analysis exists. Requesting analysis for that episode outside its session is refused.
5. With slots set to 1, a second manifest is refused "station full" with the first session's deadline; after the first completes, it is accepted.
6. With time-to-live set to 1 minute, an unfulfilled manifest is Expired within 2 minutes, the slot is reusable, and the reason is on the board.
7. With the provider forced to fail, the session is Failed with the provider reason, the episode is stored, and an operator retry completes it with a single analysis and no expiry along the way. Abandoning a different Failed session frees its season/episode for re-announcement.
8. Restart with one Ready and one Processing session: the Ready one keeps its deadline and accepts delivery; the Processing one is Failed "interrupted by restart" and retryable.
9. Announcing an existing or already-On-Air episode is refused naming the conflict; cancelling an on-air manifest frees the slot and a later delivery is refused.
10. The viewer shows an undelivered episode marked Expected, a Failed episode as not yet analyzed, and a completed episode's continuity notes and flags.

## What the viewer shows

Minimum outcomes, not designs:

- **Season list:** announced episodes appear in their season with a state marker (Expected, Processing, Failed, Expired, Cancelled) and deadline. Expected entries are not clickable. Expired and Cancelled markers stay until the pair is re-announced or a configurable period passes (default 7 days).
- **Episode details:** an episode whose session is not Completed shows its details and a prominent "Not yet analyzed" state with the reason ("Failed: retry available") and no analysis sections. A completed episode adds a "Broadcast" section with the manifest, session timeline, cold marker, and flags in plain language ("Announced Jake; transcript also featured BMO"), and a continuity section, each note linking to the prior episodes it names.
- **Character page:** continuity notes naming this character, in (season, episode number) order, so "third episode in a row where Finn doubts himself" reads as a run.
- **Station board:** "2 of 3 on air," live sessions with deadlines, recent terminal sessions and refusals with reasons. Read-only except cancel, retry, and abandon.

## Out of scope for v1 / later

- **v2:** waiting list when full; amending manifests (FR-6); hold-for-review for serious flags; deadline extension; holding a delivery that arrives during Preparing; creating manifests from the viewer; character aliasing; refreshing history between preparation and processing; bulk season announcement; backfilling notes for grandfathered episodes; changing the slot count while running.
- **Later or never:** authentication; notifications; live push; streaming or partial delivery; multiple stations; any runtime bypass of the gate.

## Open questions for the developer

1. **How retry and abandon are exposed to the operator.** Your call, as long as FR-25, FR-27, and FR-28 hold and session state stays truthful.
2. **How expiry and the processing timeout are enforced while running.** Any mechanism satisfying FR-31, FR-36, and FR-26 is fine; it must not depend on someone querying.
3. **Where history lives during a session.** Your call; the constraint is that a recovered or retried session reflects history as of its most recent preparation, and FR-21's record stays truthful.
4. **How continuity notes are produced.** One provider request or a separate step, your call, provided FR-23 holds so one failure means one retry.
5. **How the grandfather import works.** Your call within FR-15: it must be impossible to invoke while the station serves requests.
6. **Defaults.** 3 slots, 30-minute time-to-live, 10-minute processing timeout, 30-day retention are suggestions; pick what makes manual testing pleasant. If FR-19 fires on every fan-written episode, drop it to COULD.

## Glossary

- **Manifest** — The announcement that an episode is coming; identity and description, never content.
- **Session** — The station live for one manifest; holds a slot, prepares, watches the deadline, receives.
- **Episode** — The full content, stored as today, admitted only through a session.
- **History** — Analyzed episodes preceding the announced one in (season, episode number) order.
- **Flag** — A recorded, non-fatal discrepancy between manifest and content.
- **Cold session** — Went Ready without successfully loaded history; its output is labeled so.
- **Continuity note** — An analysis observation that depends on history known at preparation time.
- **Abandon** — Operator action closing a Failed session and discarding its episode.
- **Grandfathered episode** — One of the 21 pre-station episodes; exempt from the manifest rule; counts as history.
- **Grandfather import** — The seed-only, offline way grandfathered content enters.
- **Stand-in provider** — A fixed-output provider used for acceptance so results are repeatable.
