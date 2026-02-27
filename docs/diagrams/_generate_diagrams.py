#!/usr/bin/env python3
"""Generate draw.io XML files for Threaded Build Process Flow diagrams.

Layout strategy: strict vertical tree layout.
- Main flow runs down a single center column.
- Side branches (sub-details) are placed in a dedicated right column
  with Y positions coordinated to avoid overlap.
- Next main-flow node never starts until both columns are clear.
"""

import os
import html as html_mod

OUTPUT_DIR = os.path.dirname(os.path.abspath(__file__))

# Style constants
S_HEADER = "rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontStyle=1;fontSize=12;"
S_PROCESS = "rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;fontSize=11;"
S_ALT = "rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;fontSize=11;"
S_EXEC = "rounded=1;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;fontSize=11;"
S_ERROR = "rounded=1;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;fontSize=11;"
S_SUB = "rounded=1;whiteSpace=wrap;html=1;fillColor=#f5f5f5;strokeColor=#666666;fontSize=10;"
S_EDGE = "edgeStyle=orthogonalEdgeStyle;rounded=1;orthogonalLoop=1;jettySize=auto;html=1;"

# Layout constants
VGAP = 60       # vertical gap between rows
HGAP = 80       # horizontal gap between columns
NH = 50         # default node height
SH = 40         # sub-node height
SGAP = 10       # vertical gap between sub-nodes

def esc(t):
    return html_mod.escape(str(t))

def mk_cell(cid, val, x, y, w, h, style):
    return (f'        <mxCell id="{cid}" value="{esc(val)}" style="{style}" '
            f'vertex="1" parent="1">\n'
            f'          <mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry" />\n'
            f'        </mxCell>\n')

def mk_edge(eid, src, tgt, label="", style=S_EDGE):
    la = f' value="{esc(label)}"' if label else ''
    return (f'        <mxCell id="{eid}"{la} style="{style}" '
            f'edge="1" parent="1" source="{src}" target="{tgt}">\n'
            f'          <mxGeometry relative="1" as="geometry" />\n'
            f'        </mxCell>\n')

def wrap(name, body, pw=1100, ph=1200):
    return (f'<?xml version="1.0" encoding="UTF-8"?>\n'
            f'<mxfile host="app.diagrams.net" type="device">\n'
            f'  <diagram name="{esc(name)}" id="d1">\n'
            f'    <mxGraphModel dx="1422" dy="900" grid="1" gridSize="10" guides="1" '
            f'tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" '
            f'pageWidth="{pw}" pageHeight="{ph}" math="0" shadow="0">\n'
            f'      <root>\n'
            f'        <mxCell id="0" />\n'
            f'        <mxCell id="1" parent="0" />\n'
            f'{body}'
            f'      </root>\n'
            f'    </mxGraphModel>\n'
            f'  </diagram>\n'
            f'</mxfile>')

def save(fn, content):
    p = os.path.join(OUTPUT_DIR, fn)
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Created {fn}")


# ============================================================
# Diagram 1: High-Level Architecture
# ============================================================
def diagram_high_level():
    b = ""
    # Centered layout: all nodes centered around x=350 midpoint
    wt = 580       # top header width
    w3 = 190       # width for 3-column nodes
    ws = 280       # width for single-column chain nodes
    mid = 350      # horizontal center

    y = 30
    b += mk_cell("h1", "ThreadedManager\nExecuteAsync()", mid - wt//2, y, wt, 60, S_HEADER)
    y += 60 + VGAP

    # Three parallel boxes, evenly spaced
    gap3 = 20
    total3 = 3 * w3 + 2 * gap3
    x0 = mid - total3//2
    b += mk_cell("v1", "Validation &amp;\nPath Setup",            x0, y, w3, 60, S_PROCESS)
    b += mk_cell("v2", "Script Source Config\n(SBM / DACPAC / Scripts)", x0 + w3 + gap3, y, w3, 60, S_PROCESS)
    b += mk_cell("v3", "Build Preparation\n&amp; Script Loading",  x0 + 2*(w3 + gap3), y, w3, 60, S_PROCESS)
    b += mk_edge("e1", "h1", "v1")
    b += mk_edge("e2", "h1", "v2")
    b += mk_edge("e3", "h1", "v3")
    y += 60 + VGAP

    # Execution Mode label
    b += mk_cell("lbl1", "Execution Mode", mid - 80, y, 160, 25,
                 "text;html=1;align=center;verticalAlign=middle;resizable=0;points=[];autosize=1;strokeColor=none;fillColor=none;fontStyle=2;fontSize=11;")
    y += 25 + 20

    # Two execution modes
    w2 = 260
    gap2 = 40
    total2 = 2 * w2 + gap2
    x0_2 = mid - total2//2
    b += mk_cell("ex1", "ExecuteFromOverrideFile\n(Target file sourced)", x0_2, y, w2, 60, S_ALT)
    b += mk_cell("ex2", "ExecuteFromQueue\n(Service Bus sourced)",        x0_2 + w2 + gap2, y, w2, 60, S_ALT)
    b += mk_edge("e4", "v2", "ex1")
    b += mk_edge("e5", "v2", "ex2")
    y += 60 + VGAP

    # Vertical chain
    chain = [
        ("pc", "ProcessConcurrencyBucket\n(Parallel per bucket)", S_EXEC),
        ("tr", "ThreadedRunner\nRunDatabaseBuild()", S_EXEC),
        ("sh", "SqlBuildHelper\nProcessBuild()", S_EXEC),
        ("so", "SqlBuildOrchestrator\nExecute()", S_EXEC),
        ("sr", "SqlBuildRunner\nRun()", S_EXEC),
        ("sc", "SqlCommandExecutor\nExecute()", S_EXEC),
    ]
    prev = None
    for i, (cid, label, style) in enumerate(chain):
        b += mk_cell(cid, label, mid - ws//2, y, ws, 55, style)
        if i == 0:
            b += mk_edge("ec0a", "ex1", cid)
            b += mk_edge("ec0b", "ex2", cid)
        else:
            b += mk_edge(f"ec{i}", prev, cid)
        prev = cid
        y += 55 + 40

    save("01_high_level_architecture.drawio", wrap("High-Level Architecture", b, 750, y + 30))


# ============================================================
# Diagram 2: Phase 1 - Initialization & Validation
# ============================================================
def diagram_phase1():
    b = ""
    mw = 300           # main column width
    sw = 230           # sub column width
    mx = 40            # main column x
    sx = mx + mw + HGAP  # sub column x (right of main)

    y = 30
    b += mk_cell("p1h", "ThreadedManager.ExecuteAsync()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    # Step 1: Initialize ThreadedLogging (no subs)
    b += mk_cell("p1a", "Initialize ThreadedLogging\nwith RunId", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p1a", "p1h", "p1a")
    y += NH + VGAP

    # Step 2: Set root logging path (3 subs)
    b += mk_cell("p1b", "Set root logging path", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p1b", "p1a", "p1b")
    sy = y  # subs start aligned with this node
    subs_b = [
        ("p1b1", "AZ_BATCH_TASK_WORKING_DIR\n(if in Batch)"),
        ("p1b2", "cmdLine.RootLoggingPath\n(if specified)"),
        ("p1b3", "Default: ./tmp-sqlbuildlogging"),
    ]
    for cid, label in subs_b:
        b += mk_cell(cid, label, sx, sy, sw, SH, S_SUB)
        b += mk_edge(f"e_{cid}", "p1b", cid)
        sy += SH + SGAP
    y = max(y + NH, sy) + VGAP  # advance past both columns

    # Step 3: SetRootAndWorkingPaths (2 subs)
    b += mk_cell("p1c", "SetRootAndWorkingPaths()", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p1c", "p1b", "p1c")
    sy = y
    subs_c = [
        ("p1c1", "Create root logging directory"),
        ("p1c2", "Create Working subdirectory"),
    ]
    for cid, label in subs_c:
        b += mk_cell(cid, label, sx, sy, sw, SH, S_SUB)
        b += mk_edge(f"e_{cid}", "p1c", cid)
        sy += SH + SGAP
    y = max(y + NH, sy) + VGAP

    # Step 4: Validation (2 subs)
    b += mk_cell("p1d", "Validation.ValidateCommon\nCommandLineArgs()", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p1d", "p1c", "p1d")
    sy = y
    subs_d = [
        ("p1d1", "Validate required parameters"),
        ("p1d2", "Return error codes if\nvalidation fails"),
    ]
    styles_d = [S_SUB, S_ERROR]
    for (cid, label), st in zip(subs_d, styles_d):
        b += mk_cell(cid, label, sx, sy, sw, SH, st)
        b += mk_edge(f"e_{cid}", "p1d", cid)
        sy += SH + SGAP
    y = max(y + NH, sy) + 20

    save("02_phase1_initialization.drawio", wrap("Phase 1 - Initialization", b, sx + sw + 40, y))


# ============================================================
# Diagram 3: Phase 2 - Script Source Configuration
# ============================================================
def diagram_phase2():
    b = ""
    mw = 260           # main (decision) column width
    sw = 240           # sub column width
    mx = 40
    sx = mx + mw + HGAP

    y = 30
    b += mk_cell("p2h", "ConfigureScriptSource()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    decisions = [
        ("p2d1", "ScriptSrcDir specified?", [
            ("p2d1a", "ConstructBuildFileFrom\nScriptDirectory()", S_PROCESS),
            ("p2d1b", "Enumerate .sql files", S_SUB),
            ("p2d1c", "Sort alphabetically", S_SUB),
            ("p2d1d", "Build SBM package", S_SUB),
        ]),
        ("p2d2", "BuildFileName (.sbm)\nspecified?", [
            ("p2d2a", "Use SBM file directly\nas build source", S_PROCESS),
        ]),
        ("p2d3", "PlatinumDacpac\nspecified?", [
            ("p2d3a", "Use DACPAC to generate\nscripts via delta comparison", S_PROCESS),
        ]),
        ("p2d4", "PlatinumDbSource +\nPlatinumServerSource?", [
            ("p2d4a", "DacPacHelper.ExtractDacPac()", S_PROCESS),
            ("p2d4b", "Connect to platinum DB", S_SUB),
            ("p2d4c", "Extract DACPAC from\nlive database", S_SUB),
        ]),
    ]

    prev = "p2h"
    for did, dlabel, children in decisions:
        b += mk_cell(did, dlabel, mx, y, mw, NH, S_ALT)
        b += mk_edge(f"e_{did}", prev, did)

        # Place children vertically in the sub column
        sy = y
        cprev = did
        for i, (cid, clabel, cstyle) in enumerate(children):
            b += mk_cell(cid, clabel, sx, sy, sw, 45, cstyle)
            if i == 0:
                b += mk_edge(f"e_{cid}", did, cid, "Yes")
            else:
                b += mk_edge(f"e_{cid}", cprev, cid)
            cprev = cid
            sy += 45 + SGAP

        prev = did
        # Advance y past whichever column is taller
        y = max(y + NH, sy) + VGAP

    save("03_phase2_script_source.drawio", wrap("Phase 2 - Script Source Config", b, sx + sw + 40, y))


# ============================================================
# Diagram 4: Phase 3 - Build Preparation
# ============================================================
def diagram_phase3():
    b = ""
    mw = 320
    sw = 260
    mx = 40
    sx = mx + mw + HGAP

    y = 30
    b += mk_cell("p3h", "PrepBuildAndScriptsAsync()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    # ExtractAndLoadBuildFileAsync + 2 subs
    b += mk_cell("p3a", "ExtractAndLoadBuildFileAsync()", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p3a", "p3h", "p3a")
    sy = y
    subs = [
        ("p3a1", "ExtractSqlBuildZipFileAsync()\nExtract .sbm (zip) to working dir"),
        ("p3a2", "LoadSqlBuildProjectFileAsync()\nDeserialize XML project file"),
    ]
    for cid, label in subs:
        b += mk_cell(cid, label, sx, sy, sw, 45, S_SUB)
        b += mk_edge(f"e_{cid}", "p3a", cid)
        sy += 45 + SGAP
    y = max(y + NH, sy) + VGAP

    # LoadAndBatchSqlScripts + 3 subs
    b += mk_cell("p3b", "_scriptBatcher.LoadAndBatchSqlScripts()", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p3b", "p3a", "p3b")
    sy = y
    subs2 = [
        ("p3b1", "Parse scripts with\nGO batch separators"),
        ("p3b2", "Apply token replacements"),
        ("p3b3", "Store in BuildExecutionContext\n.BatchCollection"),
    ]
    for cid, label in subs2:
        b += mk_cell(cid, label, sx, sy, sw, SH, S_SUB)
        b += mk_edge(f"e_{cid}", "p3b", cid)
        sy += SH + SGAP
    y = max(y + NH, sy) + 20

    save("04_phase3_build_preparation.drawio", wrap("Phase 3 - Build Preparation", b, sx + sw + 40, y))


# ============================================================
# Diagram 5: Phase 4 - Concurrent Execution
# ============================================================
def diagram_phase4():
    b = ""
    mw = 300
    sw = 260
    mx = 40
    sx = mx + mw + HGAP

    y = 30
    b += mk_cell("p4h", "ExecuteFromOverrideFileAsync()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    # ConcurrencyByType + 4 type subs
    b += mk_cell("p4a", "Concurrency.ConcurrencyByType()", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_p4a", "p4h", "p4a")
    sy = y
    types = [
        ("ct1", "Count: Fixed # of parallel tasks"),
        ("ct2", "Server: Group by server"),
        ("ct3", "MaxPerServer: Limit per server"),
        ("ct4", "Tag: Group by concurrency tag"),
    ]
    for cid, label in types:
        b += mk_cell(cid, label, sx, sy, sw, 35, S_SUB)
        b += mk_edge(f"e_{cid}", "p4a", cid)
        sy += 35 + SGAP
    y = max(y + NH, sy) + VGAP

    # Remaining linear steps
    steps = [
        ("p4b", "For each bucket (parallel):\nProcessConcurrencyBucketAsync()", S_EXEC, 55),
        ("p4c", "For each (server, overrides):\nCreate ThreadedRunner", S_EXEC, 55),
        ("p4d", "ProcessThreadedBuildAsync()", S_PROCESS, NH),
        ("p4e", "await Task.WhenAll(tasks)\nAggregate results", S_HEADER, 55),
    ]
    prev = "p4a"
    for cid, label, style, h in steps:
        b += mk_cell(cid, label, mx, y, mw, h, style)
        b += mk_edge(f"e_{cid}", prev, cid)
        prev = cid
        y += h + VGAP

    save("05_phase4_concurrent_execution.drawio", wrap("Phase 4 - Concurrent Execution", b, sx + sw + 40, y))


# ============================================================
# Diagram 6: Phase 4b - Queue-based Execution
# ============================================================
def diagram_phase4b():
    b = ""
    mw = 320
    sw = 250
    mx = 40
    sx = mx + mw + HGAP

    y = 30
    b += mk_cell("q1", "ExecuteFromQueueAsync()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    b += mk_cell("q2", "QueueManager\n.GetDatabaseTargetFromQueue()\nReceive messages from Service Bus", mx, y, mw, 65, S_PROCESS)
    b += mk_edge("e_q2", "q1", "q2")
    y += 65 + VGAP

    b += mk_cell("q3", "For each message (parallel):\nCreate ThreadedRunner", mx, y, mw, 55, S_EXEC)
    b += mk_edge("e_q3", "q2", "q3")
    y += 55 + VGAP

    # ProcessThreadedBuildWithQueueAsync + 3 subs
    b += mk_cell("q4", "ProcessThreadedBuild\nWithQueueAsync()", mx, y, mw, 55, S_PROCESS)
    b += mk_edge("e_q4", "q3", "q4")
    sy = y
    subs = [
        ("q4a", "ProcessThreadedBuildAsync()", S_EXEC),
        ("q4b", "Renew message lock\nevery 30 seconds", S_SUB),
        ("q4c", "CompleteMessage() or\nDeadletterMessage()", S_ALT),
    ]
    for cid, label, style in subs:
        b += mk_cell(cid, label, sx, sy, sw, 45, style)
        b += mk_edge(f"e_{cid}", "q4", cid)
        sy += 45 + SGAP
    y = max(y + 55, sy) + VGAP

    b += mk_cell("q5", "Loop until no messages\n(with retry logic)", mx, y, mw, NH, S_ALT)
    b += mk_edge("e_q5", "q4", "q5")
    y += NH + 20

    save("06_phase4b_queue_execution.drawio", wrap("Phase 4b - Queue Execution", b, sx + sw + 40, y))


# ============================================================
# Diagram 7: Phase 5 - Script Execution per Database
# ============================================================
def diagram_phase5():
    b = ""
    mw = 300
    sw = 280
    mx = 60
    sx = mx + mw + HGAP

    y = 30
    b += mk_cell("s1", "ThreadedRunner\n.RunDatabaseBuildAsync()", mx, y, mw, 55, S_HEADER)
    y += 55 + VGAP

    b += mk_cell("s2", "Build SqlBuildRunDataModel\n(IsTrial, IsTransactional, Overrides)", mx, y, mw, 55, S_PROCESS)
    b += mk_edge("e_s2", "s1", "s2")
    y += 55 + VGAP

    # ForceCustomDacpac decision + 1 large sub
    b += mk_cell("s3", "ForceCustomDacpac?", mx, y, mw, 45, S_ALT)
    b += mk_edge("e_s3", "s2", "s3")
    b += mk_cell("s3a", "DacPacHelper\n.UpdateBuildRunDataForDacPacSync()\n- Extract target schema\n- Compare with platinum DACPAC\n- Generate delta scripts", sx, y, sw, 90, S_SUB)
    b += mk_edge("e_s3a", "s3", "s3a", "Yes")
    y = max(y + 45, y + 90) + VGAP

    # Linear chain of execution steps
    linear = [
        ("s4",  "SqlBuildHelper.ProcessBuildAsync()",                  S_PROCESS, 45),
        ("s5",  "PrepareBuildForRunAsync()\nFilter scripts, apply overrides", S_PROCESS, 55),
        ("s6",  "SqlBuildOrchestrator.ExecuteAsync()",                 S_EXEC, 45),
        ("s7",  "SqlBuildRunner.RunAsync()",                           S_EXEC, 45),
        ("s8",  "Open connection\nBegin transaction (if transactional)", S_PROCESS, 55),
        ("s9",  "For each script in\nbatch collection:",               S_EXEC, 55),
        ("s10", "Create savepoint\n(for rollback granularity)",        S_PROCESS, 55),
        ("s11", "SqlCommandExecutor.ExecuteAsync()\nSet timeout, ExecuteNonQueryAsync()", S_EXEC, 55),
    ]
    prev = "s3"
    for cid, label, style, h in linear:
        b += mk_cell(cid, label, mx, y, mw, h, style)
        b += mk_edge(f"e_{cid}", prev, cid)
        prev = cid
        y += h + 40

    # Success / Failure fork
    fw = 240
    fgap = 40
    total = 2 * fw + fgap
    fx0 = mx + mw//2 - total//2
    b += mk_cell("s12", "On success:\nAdd to committedScripts list",  fx0, y, fw, 55, S_PROCESS)
    b += mk_cell("s13", "On failure:\nHandleSqlException()\nRollback to savepoint", fx0 + fw + fgap, y, fw, 65, S_ERROR)
    b += mk_edge("e_s12", "s11", "s12", "Success")
    b += mk_edge("e_s13", "s11", "s13", "Failure")
    y += 65 + VGAP

    b += mk_cell("s14", "Return BuildStatus", mx + mw//2 - 120, y, 240, 45, S_HEADER)
    b += mk_edge("e_s14a", "s12", "s14")
    b += mk_edge("e_s14b", "s13", "s14")
    y += 45 + 20

    save("07_phase5_script_execution.drawio", wrap("Phase 5 - Script Execution", b, sx + sw + 40, y))


# ============================================================
# Diagram 8: Phase 6 - Transaction Handling
# ============================================================
def diagram_phase6():
    b = ""
    # Three columns: left (success/trial), center (decision), right (failure)
    cw = 270           # column width
    cgap = 60          # gap between columns
    lx = 20            # left column x
    cx = lx + cw + cgap   # center column x
    rx = cx + cw + cgap    # right column x

    y = 30
    b += mk_cell("t1", "SqlBuildRunner.RunAsync()\n(continued)", cx, y, cw, NH, S_HEADER)
    y += NH + VGAP

    b += mk_cell("t2", "All scripts succeeded?", cx, y, cw, 45, S_ALT)
    b += mk_edge("e_t2", "t1", "t2")
    fork_y = y + 45 + VGAP

    # ---- YES branch (left column) ----
    yy = fork_y
    b += mk_cell("t3", "IsTrial = true?", lx, yy, cw, 45, S_ALT)
    b += mk_edge("e_t3", "t2", "t3", "Yes")
    yy += 45 + VGAP

    b += mk_cell("t3a", "Transaction.Rollback()\nBuildSuccessTrialRolledBackEvent", lx, yy, cw, 55, S_PROCESS)
    b += mk_edge("e_t3a", "t3", "t3a", "Yes")
    yy += 55 + VGAP

    b += mk_cell("t4", "IsTrial = false?", lx, yy, cw, 45, S_ALT)
    b += mk_edge("e_t4", "t3", "t4", "No")
    yy += 45 + VGAP

    b += mk_cell("t4a", "DefaultBuildFinalizer\n.PerformRunScriptFinalizationAsync()", lx, yy, cw, 55, S_EXEC)
    b += mk_edge("e_t4a", "t4", "t4a")
    yy += 55 + 30

    fin_steps = [
        ("f1", "CommitBuild()\nTransaction.Commit()", S_PROCESS),
        ("f2", "RecordCommittedScripts()", S_PROCESS),
        ("f3", "LogCommittedScriptsToDatabase()\nWrite to SqlBuild_Logging", S_PROCESS),
        ("f4", "SaveBuildDataModelAsync()", S_PROCESS),
        ("f5", "RaiseBuildCommittedEvent()", S_PROCESS),
    ]
    for cid, label, style in fin_steps:
        b += mk_cell(cid, label, lx, yy, cw, 45, style)
        b += mk_edge(f"e_{cid}", "t4a", cid)
        yy += 45 + SGAP
    left_bottom = yy

    # ---- NO branch (right column) ----
    ny = fork_y
    b += mk_cell("t5", "Any script failed?", rx, ny, cw, 45, S_ERROR)
    b += mk_edge("e_t5", "t2", "t5", "No")
    ny += 45 + VGAP

    b += mk_cell("t5a", "IsTransactional = true?", rx, ny, cw, 45, S_ALT)
    b += mk_edge("e_t5a", "t5", "t5a")
    ny += 45 + VGAP

    b += mk_cell("t5b", "Transaction.Rollback()\nBuildErrorRollBackEvent", rx, ny, cw, 55, S_ERROR)
    b += mk_edge("e_t5b", "t5a", "t5b", "Yes")
    ny += 55 + VGAP

    b += mk_cell("t5c", "Partial commit\nBuildErrorNonTransactionalEvent", rx, ny, cw, 55, S_ERROR)
    b += mk_edge("e_t5c", "t5a", "t5c", "No")
    right_bottom = ny + 55

    y = max(left_bottom, right_bottom) + 20
    save("08_phase6_transaction_handling.drawio", wrap("Phase 6 - Transaction Handling", b, rx + cw + 40, y))


# ============================================================
# Diagram 9: Commit Flow Architecture
# ============================================================
def diagram_commit_flow():
    b = ""
    wt = 600
    w3 = 190
    gap3 = 15
    total3 = 3 * w3 + 2 * gap3
    mid = 330
    x0 = mid - total3//2

    y = 30
    b += mk_cell("cf1", "DefaultBuildFinalizer\nPerformRunScriptFinalizationAsync()", mid - wt//2, y, wt, 60, S_HEADER)
    y += 60 + VGAP

    b += mk_cell("cf2", "CommitBuild()\nTransaction.Commit",      x0, y, w3, 55, S_PROCESS)
    b += mk_cell("cf3", "RecordCommittedScripts\nPopulate POCO list", x0 + w3 + gap3, y, w3, 55, S_PROCESS)
    b += mk_cell("cf4", "SaveBuildDataModel\nPersist to file",      x0 + 2*(w3 + gap3), y, w3, 55, S_PROCESS)
    b += mk_edge("e_cf2", "cf1", "cf2")
    b += mk_edge("e_cf3", "cf1", "cf3")
    b += mk_edge("e_cf4", "cf1", "cf4")
    y += 55 + VGAP

    b += mk_cell("cf5", "DefaultSqlLoggingService\nLogCommittedScriptsToDatabase()", mid - 190, y, 380, 55, S_EXEC)
    b += mk_edge("e_cf5", "cf3", "cf5")
    y += 55 + VGAP

    b += mk_cell("cf6", "EnsureLogTableExists\nCreate if missing",  x0, y, w3, 55, S_PROCESS)
    b += mk_cell("cf7", "Group scripts by DB\nconnection",           x0 + w3 + gap3, y, w3, 55, S_PROCESS)
    b += mk_cell("cf8", "Batch INSERT or\nfallback to single",      x0 + 2*(w3 + gap3), y, w3, 55, S_ALT)
    b += mk_edge("e_cf6", "cf5", "cf6")
    b += mk_edge("e_cf7", "cf5", "cf7")
    b += mk_edge("e_cf8", "cf5", "cf8")
    y += 55 + 20

    save("09_commit_flow_architecture.drawio", wrap("Commit Flow Architecture", b, 700, y))


# ============================================================
# Diagram 10: Detailed Commit Process
# ============================================================
def diagram_detailed_commit():
    b = ""
    mw = 340
    sw = 220
    mx = 40
    sx = mx + mw + HGAP

    y = 30
    b += mk_cell("dc1", "PerformRunScriptFinalizationAsync()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    b += mk_cell("dc2", "CommitBuild()\nTransaction.Commit() + Close connection", mx, y, mw, 55, S_PROCESS)
    b += mk_edge("e_dc2", "dc1", "dc2")
    y += 55 + VGAP

    # RecordCommittedScripts + 5 field subs
    b += mk_cell("dc3", "RecordCommittedScripts(committedScripts)", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_dc3", "dc2", "dc3")
    sy = y
    fields = [
        "ScriptId (GUID as string)",
        "ServerName",
        "CommittedDate (DateTime.Now)",
        "ScriptHash (FileHash)",
        "SqlSyncBuildProjectId",
    ]
    for i, label in enumerate(fields):
        cid = f"dc3{chr(97+i)}"
        b += mk_cell(cid, label, sx, sy, sw, 30, S_SUB)
        b += mk_edge(f"e_{cid}", "dc3", cid)
        sy += 30 + 8
    y = max(y + NH, sy) + VGAP

    # LogCommittedScriptsToDatabase
    b += mk_cell("dc4", "DefaultSqlLoggingService\n.LogCommittedScriptsToDatabase()", mx, y, mw, 55, S_EXEC)
    b += mk_edge("e_dc4", "dc3", "dc4")
    y += 55 + VGAP

    # Linear sub-steps
    log_steps = [
        ("dc4a", "EnsureLogTablePresence()\nCheck cache, CREATE TABLE if needed", S_PROCESS),
        ("dc4b", "Group CommittedScripts by\n(Server, Database)", S_PROCESS),
        ("dc4c", "Try batch INSERT\n(multi-row VALUES)", S_PROCESS),
        ("dc4d", "On failure: fallback to\nindividual INSERTs", S_ERROR),
    ]
    prev_cid = "dc4"
    for cid, label, style in log_steps:
        b += mk_cell(cid, label, mx, y, mw, NH, style)
        b += mk_edge(f"e_{cid}", prev_cid, cid)
        prev_cid = cid
        y += NH + 40

    # SaveBuildDataModelAsync
    b += mk_cell("dc5", "SaveBuildDataModelAsync()\nPersist to project XML file", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_dc5", "dc4", "dc5")
    y += NH + 20

    save("10_detailed_commit_process.drawio", wrap("Detailed Commit Process", b, sx + sw + 40, y))


# ============================================================
# Diagram 11: Error Handling in Logging
# ============================================================
def diagram_error_handling():
    b = ""
    mw = 300
    mid = 200
    mx = mid - mw//2 + 50

    y = 30
    b += mk_cell("eh1", "LogCommittedScriptsToDatabase()", mx, y, mw, NH, S_HEADER)
    y += NH + VGAP

    b += mk_cell("eh2", "Try: Batch INSERT\n(all scripts in one statement)\nMulti-row VALUES clause", mx, y, mw, 65, S_PROCESS)
    b += mk_edge("e_eh2", "eh1", "eh2")
    y += 65 + VGAP

    b += mk_cell("eh3", "Catch: Fallback to\nindividual INSERTs", mx, y, mw, NH, S_ERROR)
    b += mk_edge("e_eh3", "eh2", "eh3", "Exception")
    y += NH + VGAP

    b += mk_cell("eh4", "For each script:", mx, y, mw, 40, S_EXEC)
    b += mk_edge("e_eh4", "eh3", "eh4")
    y += 40 + VGAP

    # Two branches side by side
    fw = 240
    fgap = 50
    total = 2 * fw + fgap
    fx0 = mx + mw//2 - total//2
    b += mk_cell("eh5", "Try: Single parameterized INSERT", fx0, y, fw, 45, S_PROCESS)
    b += mk_cell("eh6", "Catch: Log error,\ncontinue with next script", fx0 + fw + fgap, y, fw, 45, S_ERROR)
    b += mk_edge("e_eh5", "eh4", "eh5")
    b += mk_edge("e_eh6", "eh5", "eh6", "Exception")
    y += 45 + 20

    save("11_error_handling_logging.drawio", wrap("Error Handling in Logging", b, fx0 + 2*fw + fgap + 40, y))


# ============================================================
# Diagram 12: Event Flow
# ============================================================
def diagram_event_flow():
    b = ""
    mw = 290
    mid = 350
    mx = mid - mw//2

    y = 30
    b += mk_cell("ev1", "Build Start", mx, y, mw, 45, S_HEADER)
    y += 45 + VGAP

    b += mk_cell("ev2", 'ThreadedLogging.WriteToLog\n(Message: "Starting thread")', mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_ev2", "ev1", "ev2")
    y += NH + VGAP

    b += mk_cell("ev3", "ScriptLogWriteEvent\n(per script, if EventHub enabled)", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_ev3", "ev2", "ev3")
    y += NH + VGAP

    # Three outcome events
    ew = 200
    egap = 20
    total3 = 3 * ew + 2 * egap
    ex0 = mid - total3//2
    b += mk_cell("ev4", "BuildCommittedEvent",           ex0, y, ew, 45, S_PROCESS)
    b += mk_cell("ev5", "BuildErrorRollBackEvent",       ex0 + ew + egap, y, ew, 45, S_ERROR)
    b += mk_cell("ev6", "BuildSuccess\nTrialRolledBackEvent", ex0 + 2*(ew + egap), y, ew, 45, S_ALT)
    b += mk_edge("e_ev4", "ev3", "ev4")
    b += mk_edge("e_ev5", "ev3", "ev5")
    b += mk_edge("e_ev6", "ev3", "ev6")
    y += 45 + VGAP

    b += mk_cell("ev7", "ThreadedLogging.WriteToLog\n(Commit/Error log)", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_ev7a", "ev4", "ev7")
    b += mk_edge("e_ev7b", "ev5", "ev7")
    b += mk_edge("e_ev7c", "ev6", "ev7")
    y += NH + VGAP

    b += mk_cell("ev8", "EventHub submission\n(if configured)", mx, y, mw, 45, S_EXEC)
    b += mk_edge("e_ev8", "ev7", "ev8")
    y += 45 + VGAP

    # Three destinations
    dw = 160
    dgap = 20
    total_d = 3 * dw + 2 * dgap
    dx0 = mid - total_d//2
    b += mk_cell("ev9",  "Local log files",  dx0, y, dw, 35, S_SUB)
    b += mk_cell("ev10", "Azure Event Hub",  dx0 + dw + dgap, y, dw, 35, S_SUB)
    b += mk_cell("ev11", "Console output",   dx0 + 2*(dw + dgap), y, dw, 35, S_SUB)
    b += mk_edge("e_ev9", "ev8", "ev9")
    b += mk_edge("e_ev10", "ev8", "ev10")
    b += mk_edge("e_ev11", "ev8", "ev11")
    y += 35 + 20

    save("12_event_flow.drawio", wrap("Event Flow", b, ex0 + total3 + 40, y))


# ============================================================
# Diagram 13: Concurrency Model
# ============================================================
def diagram_concurrency():
    b = ""
    mid = 310
    tw = 420
    bw = 160
    bgap = 30
    total_b = 3 * bw + 2 * bgap
    bx0 = mid - total_b//2

    y = 30
    b += mk_cell("cm1", "Database Target List\nServer1:DB1, Server1:DB2, Server2:DB3", mid - tw//2, y, tw, 55, S_HEADER)
    y += 55 + VGAP

    b += mk_cell("cm2", "Bucket 1\nServer1:DB1", bx0, y, bw, 55, S_PROCESS)
    b += mk_cell("cm3", "Bucket 2\nServer1:DB2", bx0 + bw + bgap, y, bw, 55, S_PROCESS)
    b += mk_cell("cm4", "Bucket 3\nServer2:DB3", bx0 + 2*(bw + bgap), y, bw, 55, S_PROCESS)
    b += mk_edge("e_cm2", "cm1", "cm2")
    b += mk_edge("e_cm3", "cm1", "cm3")
    b += mk_edge("e_cm4", "cm1", "cm4")
    y += 55 + VGAP

    b += mk_cell("cm5", "Thread 1\nRunner", bx0, y, bw, 55, S_EXEC)
    b += mk_cell("cm6", "Thread 2\nRunner", bx0 + bw + bgap, y, bw, 55, S_EXEC)
    b += mk_cell("cm7", "Thread 3\nRunner", bx0 + 2*(bw + bgap), y, bw, 55, S_EXEC)
    b += mk_edge("e_cm5", "cm2", "cm5")
    b += mk_edge("e_cm6", "cm3", "cm6")
    b += mk_edge("e_cm7", "cm4", "cm7")
    y += 55 + VGAP

    b += mk_cell("cm8", "Task.WhenAll(tasks)\nAwait all completions", mid - 170, y, 340, NH, S_HEADER)
    b += mk_edge("e_cm8a", "cm5", "cm8")
    b += mk_edge("e_cm8b", "cm6", "cm8")
    b += mk_edge("e_cm8c", "cm7", "cm8")
    y += NH + 20

    save("13_concurrency_model.drawio", wrap("Concurrency Model", b, bx0 + total_b + 40, y))


# ============================================================
# Diagram 14: Alternate Database Logging
# ============================================================
def diagram_alt_logging():
    b = ""
    mw = 320
    mx = 40

    y = 30
    b += mk_cell("al1", "ThreadedRunner\n.RunDatabaseBuildAsync()", mx, y, mw, 55, S_HEADER)
    y += 55 + VGAP

    b += mk_cell("al2", "cmdArgs.LogToDatabaseName\nspecified?", mx, y, mw, 45, S_ALT)
    b += mk_edge("e_al2", "al1", "al2")
    y += 45 + VGAP

    b += mk_cell("al3", "runDataModel.LogToDatabaseName\n= cmdArgs.LogToDatabaseName", mx, y, mw, NH, S_PROCESS)
    b += mk_edge("e_al3", "al2", "al3", "Yes")
    y += NH + VGAP

    b += mk_cell("al4", "DefaultSqlLoggingService uses\nalternate connection instead of\ntarget database connection", mx, y, mw, 65, S_EXEC)
    b += mk_edge("e_al4", "al3", "al4")
    y += 65 + 20

    save("14_alternate_database_logging.drawio", wrap("Alternate Database Logging", b, mx + mw + 40, y))


# ============================================================
# Run all generators
# ============================================================
if __name__ == "__main__":
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    diagram_high_level()
    diagram_phase1()
    diagram_phase2()
    diagram_phase3()
    diagram_phase4()
    diagram_phase4b()
    diagram_phase5()
    diagram_phase6()
    diagram_commit_flow()
    diagram_detailed_commit()
    diagram_error_handling()
    diagram_event_flow()
    diagram_concurrency()
    diagram_alt_logging()
    print("\nAll diagrams generated!")
