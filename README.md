# BasilTest

BasilTest is a C# program that integrates several of the
[Herbal3d] communication libraries and acts as a simple
SpaceServer to a [Basil Viewer].

As a SpaceServer, BasilTest performs different functionality and
stress tests on the viewer.

As of 20200426, the tests are

* CreateAndDeleteDisplayable
* CreateAndDeleteInstance
* CreateTenDisplayablesAndDeleteOne
* Create125InstancesDeleteOne
* UpdateInstancePosition

Refer to the project [TODOs] for more tests to be added.

## Building

Building will eventually be in a [Docker] container and nicely
scripted so you don't have to do anything but press a button.
See [https://github.com/Herbal3d/BasilTest/projects/1#card-37044406]

Building requires the inclusion of the [Herbal3d] utilities
and transport libraries so it's a little tricky and will require
some scripting and packaging.

## Operation

Eventually operation will be spinning up a [Docker] container.
There will eventually be an always running version of BasilTest
for continuious testing and the plan is to even have a server-less
version that doesn't take up too many resources.
See [https://github.com/Herbal3d/BasilTest/projects/1#card-37044483]

[Herbal3d]: https://herbal3d.org/
[Basil Viewer]: https://github.com/Herbal3d/basil
[Docker]: https://www.docker.com/
[TODOs]: https://github.com/Herbal3d/BasilTest/projects/1#column-8917936

