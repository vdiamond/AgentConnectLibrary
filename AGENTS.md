# AGENTS.md

Development happens only through Victor and Cursor agents.

## What this repo is

AgentConnectLibrary is a shared library for [AgentConnect](https://github.com/vdiamond/AgentConnect).

Status: new. There is no code yet.

## Build, run, and test

There is no code and there are no tests yet. This repository is expected to be a .NET/C# library, under `.cursor/rules/coding-standards.mdc`. No build, run, or test command exists yet.

## Layout

There is no source layout yet. The files in the repository are `LICENSE` and `.cursor/rules/coding-standards.mdc`.

## Dependencies on other vdiamond repos

None yet.

Intended consumers, to be confirmed when code lands:

- [AgentConnect](https://github.com/vdiamond/AgentConnect)
- [AgentConnectClient](https://github.com/vdiamond/AgentConnectClient)

## Conventions

`.cursor/rules/coding-standards.mdc` is the governing standard.

## What not to touch

- Secrets. Do not commit them.
- The public API. Ask Victor before changing it.
- `.grok/` and other agent instruction formats. Agent instructions are this file and `.cursor/rules/`.
