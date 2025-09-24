# 修复窗口模糊遮挡、NoteTag 标签不显示、按键记录窗口不可见与焦点抢占治理

## Core Features

- 修复 CS8510：合并 macOS Key.Enter/Key.Return 为同一 switch 分支

- 保留前述幽灵键修复与组合键稳定策略

## Tech Stack

{
  "Desktop": "Avalonia .NET（.axaml/.cs）"
}

## Design

最小化修改以消除编译错误，不影响既有行为。

## Plan

Note: 

- [ ] is holding
- [/] is doing
- [X] is done

---

[/] Step1_窗口模糊遮挡排查与修复

[/] Step2_按键记录窗口显示修复

[/] Step3_NoteTagComponent 绑定与模板修复

[/] Step4_输入焦点抢占治理
