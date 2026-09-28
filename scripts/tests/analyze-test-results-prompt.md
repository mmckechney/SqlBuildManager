# Test Output Analysis

The folder '{{resultsPath}}' contains sub-folders named for different test run types.

These sub-folders contain `TestResults.html` test result HTML summaries and `console-output.log` console output log files.

**IMPORTANT:** In the `console-output.log` file, the log entries are organized first with the `Passed` or `Failed` message on the same line as the test name, followed by the `Standard Output Messages:` and `TestContext Messages:` lines and content.   

**Expected recovery scenarios:** SQL Server AlreadyInSync tests, including the ACI
force-custom recovery scenario, explicitly require four worker `ERR` entries across
the job's task logs. Those entries exercise the intentional base-script failure and
DACPAC recovery path; they are not by themselves a defect. Check the test's declared
`expectedTaskErrorCount` and final result assertions. Zero, fewer than four, or more
than four counted entries must fail an exact-four scenario. Existing transient
Service Bus shutdown exclusions still apply. Nonempty `errors.log`, per-target
error logs, failed-database records, or incorrect database counts remain failures.

## For Failed Tests:
- Please review these files and for all failures, create an analysis of the failures and how they can be fixed. 
- Save your analysis to a single `failures.md ` file.  

## Review of output for passed or skipped tests:
- For the tests that didn't fail, please review the logs and identify any messages that either have misleading messages or suggest something may have gone wrong, even if the test passed.
- If the test case is to test a failure path or Asserts a `ThrowsExactlyAsync`, don't bother warning me about ERR or WRN logs - I want these ERR and WRN messags in the logs/
- Create a suggestion of what might be changed to remediate your findings. 
- Be sure to include the list of effected tests with each set of recommendations
- Please create a single `observations.md` markdown file with your observations analysis. 
  
- Save both markdown files to the '{{resultsPath}}' directory.
