## Cursor Cloud specific instructions

### Overview

This is a collection of standalone Python learning scripts from Codecademy courses and personal projects. There is no build system, test framework, CI/CD, or formal dependency management.

### Structure

- `Python/Testprogramme/` — Small CLI scripts (calculator, planet weight, Magic 8 Ball). These are **interactive** and require stdin input; pipe input when running non-interactively.
- `Python/codecademy projects/` — Codecademy exercises. Most run without interaction; two are Jupyter notebooks.
- `Python/Arbeitsprogramm/` — A tkinter GUI app that extracts data from work-schedule PDFs. Requires `pdfminer.six`. The GUI (`Fertig.py`) needs a display server; `PDF extract.py` is CLI-only.

### Dependencies

The only external pip package is `pdfminer.six` (used by `Python/Arbeitsprogramm/`). All other scripts use only the Python standard library. The committed `env/` directories contain Windows virtualenvs and are not usable on Linux.

### Running scripts

Each `.py` file is independent. Run with `python3 <path>`. For interactive scripts, pipe input:
```
printf '+\n5\n3\nexit\n' | python3 "Python/Testprogramme/test.py"
```

### Caveats

- `Fertig.py` uses `tkinter` and requires a display (X11/Xvfb). It will fail in headless environments without a virtual framebuffer.
- `PDF extract.py` has a hardcoded relative path (`Python/Arbeitsprogramm/Files/stundennachweise_8_2024.pdf`) — run it from the repo root.
- There are no automated tests, no linter config, and no build step.
