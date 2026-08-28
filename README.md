<!-- [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://vshymanskyy.github.io/StandWithUkraine) -->

<div align="center">
  <br />
  <a href="https://github.com/punker76/gong-wpf-dragdrop">
    <img alt="gong-wpf-dragdrop" width="700" heigth="142" src="./GongSolutions.Wpf.DragDrop.Full.png">
  </a>
  <h1>GongSolutions.Avalonia.DragDrop</h1>
  <p>
    An easy-to-use drag-and-drop framework for Avalonia.
  </p>
  <p>
    Built with Avalonia 12.1.1 and .NET 8.
  </p>

  <a href="https://gitter.im/punker76/gong-wpf-dragdrop">
	  <img src="https://img.shields.io/badge/Gitter-Join%20Chat-green.svg?style=flat-square">
  </a>
  <a href="https://twitter.com/punker76">
	  <img src="https://img.shields.io/badge/twitter-%40punker76-55acee.svg?style=flat-square">
  </a>
  <br />
  <a href="https://ci.appveyor.com/project/punker76/gong-wpf-dragdrop/branch/main">
	  <img alt="mainstatus" src="https://img.shields.io/appveyor/ci/punker76/gong-wpf-dragdrop/main.svg?style=flat-square&&label=main">
  </a>
  <a href="https://ci.appveyor.com/project/punker76/gong-wpf-dragdrop/branch/develop">
	  <img alt="devstatus" src="https://img.shields.io/appveyor/ci/punker76/gong-wpf-dragdrop/develop.svg?style=flat-square&&label=develop">
  </a>
  <a href="https://github.com/punker76/gong-wpf-dragdrop/issues">
    <img src="https://img.shields.io/github/issues/punker76/gong-wpf-dragdrop.svg?style=flat-square">
  </a>
  <br />
  <a href="https://github.com/punker76/gong-wpf-dragdrop/releases/latest">
	  <img src="https://img.shields.io/github/release/punker76/gong-wpf-dragdrop.svg?style=flat-square">
  </a>
  <a href="https://www.nuget.org/packages/gong-wpf-dragdrop">
    <img src="https://img.shields.io/nuget/dt/gong-wpf-dragdrop.svg?style=flat-square">
  </a>
  <a href="https://www.nuget.org/packages/gong-wpf-dragdrop">
    <img src="https://img.shields.io/nuget/v/gong-wpf-dragdrop.svg?style=flat-square">
  </a>
  <a href="https://www.nuget.org/packages/gong-wpf-dragdrop">
    <img src="https://img.shields.io/nuget/vpre/gong-wpf-dragdrop.svg?style=flat-square&label=nuget-pre">
  </a>
  <br />
  <br />
</div>

## Features

+ Native Avalonia 12 pointer and asynchronous data-transfer APIs.
+ MVVM-friendly attached properties and replaceable drag/drop handlers.
+ Single and multiple selection for `ListBox`.
+ Reorder within a collection or move items between collections.
+ Hold Ctrl while dropping to copy.
+ Optional drag/drop contexts prevent unintended cross-area drops.
+ Runs on every desktop platform supported by Avalonia.

The original WPF implementation remains under `src/GongSolutions.WPF.DragDrop` and
`src/Showcase` as migration reference. The active solution contains only the native
Avalonia projects.

## Get started

```shell
dotnet build src/GongSolutions.Avalonia.DragDrop.sln
dotnet run --project src/Showcase.Avalonia/Showcase.Avalonia.DragDrop.csproj
```

Enable drag and drop in AXAML:

```xml
<ListBox ItemsSource="{Binding Items}"
         dd:DragDrop.IsDragSource="True"
         dd:DragDrop.IsDropTarget="True" />
```

## License

Copyright © Jan Karger, Steven Kirk and Contributors. All rights reserved.

`GongSolutions.Avalonia.DragDrop` is provided as-is under the BSD 3-Clause License. For more information see [LICENSE](./LICENSE).

## Want to say thanks?

This framework is free and can be used for free, open source and commercial applications. It's tested, used and contributed by many awesome people.  So hit the magic :star: button, we appreciate it!!! :pray:

[Become a sponsor](https://github.com/sponsors/punker76) and show your support to this open source project.

If you use `GongSolutions.WPF.DragDrop` as serious task, and you'd like to honor my work on it, please donate, I'll appreciate it.

Does your company use `GongSolutions.WPF.DragDrop`?  Ask your manager or marketing team if your company would be interested in supporting this project.  Your company's logo can be shown [on GitHub](https://github.com/punker76/gong-wpf-dragdrop#readme) - who doesn't want a little extra exposure?

## In action

![gif01](./screenshots/gong_240.gif)

![screenshot01](./screenshots/2016-09-03_00h51_35.png)

![screenshot02](./screenshots/2016-09-03_00h52_20.png)

![screenshot03](./screenshots/2016-09-03_00h53_03.png)

![screenshot04](./screenshots/2016-09-03_00h53_21.png)

![gif02](./screenshots/DragDropSample01.gif)

![gif03](./screenshots/DragHint-Demo.gif)
