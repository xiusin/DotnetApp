# 修复窗口模糊遮挡、NoteTag 标签不显示、按键记录窗口不可见与焦点抢占问题

## Core Features

- 回退窗口透明级别以确保稳定可见

- 保留边框半透明背景与圆角，避免完全透明导致看不见

## Tech Stack

{
  "Desktop": "Avalonia .NET（.axaml/.cs）"
}

## Design

先保证显示可靠，再逐步恢复模糊与美化；最小改动仅调整窗口透明配置。

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
