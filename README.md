BDDSharp
========

BDDSharp is a C# library for manipulating roBDDs (Reduced Ordered Binary
Decision Diagrams).

[![Build Status](https://travis-ci.org/ancailliau/BDDSharp.svg?branch=master)](https://travis-ci.org/ancailliau/BDDSharp)

Future Work
-----------

Currently, the library is not intended to be memory efficient. Implementing
complemented edges and shared BDDs might greatly improve the memory footprint.

References
----------

Christoph Meinel and Thorsten Theobald. 1998. *Algorithms and Data Structures
in VLSI Design (1st ed.).* Springer-Verlag New York, Inc., Secaucus, NJ, USA.

Randal E. Bryant. 1986. *Graph-Based Algorithms for Boolean Function
Manipulation.* IEEE Trans. Comput. 35, 8 (August 1986), 677-691.
DOI=10.1109/TC.1986.1676819 http://dx.doi.org/10.1109/TC.1986.1676819

Tony Stornetta and Forrest Brewer. 1996. *Implementation of an efficient
parallel BDD package.* In Proceedings of the 33rd annual Design Automation
Conference (DAC '96). ACM, New York, NY, USA, 641-644.
DOI=10.1145/240518.240639 http://doi.acm.org/10.1145/240518.240639

Installation
------------

Just add the project to your solution and reference it. You can also compile
it and reference ``BDDSharp.dll`.

Usage
-----

The library is intended to be simple to use and extend. Performance, even if
taken into account, is not the primary goal. The library is fully developed in
C# and requires no external dependencies (like CUDD).

To build a BDD, you can create your nodes by instancing ``BDDNode`. All
operations on BDD are located in the `BDDManager` class. When calling a
function of the manager, it is your responsibility that BDDs were created
using the same manager.

For instance, to represents the following BDD, 

![BDD](https://github.com/ancailliau/BDDSharp/wiki/img/unreduced.dot.png)

you could type the following code

```csharp
var manager = new BDDManager (3);
var n3 = new BDDNode (2, manager.One, manager.One);
var n4 = new BDDNode (1, n3, manager.Zero);
var n2 = new BDDNode (1, n3, manager.Zero);
var root = new BDDNode (0, n2, n4);
```

To reduce a BDD, you can call `BDDManager.Reduce (BDDNode)`.

```csharp
manager.Reduce (root);
```

Applied to the previous example, you ends up with the following BDD

![BDD](https://github.com/ancailliau/BDDSharp/wiki/img/reduced.dot.png)

To combine multiple BDDs using the ITE operator, just call the corresponding
function

```csharp
manager.ite (f, g, h)
```

For instance, consider the following BDDs (f, g and h respectively)

![BDD](https://github.com/ancailliau/BDDSharp/wiki/img/f.dot.png)
![BDD](https://github.com/ancailliau/BDDSharp/wiki/img/g.dot.png)
![BDD](https://github.com/ancailliau/BDDSharp/wiki/img/h.dot.png)

The resulting BDDs returned by applying ITE operator is given by

![BDD](https://github.com/ancailliau/BDDSharp/wiki/img/ite.dot.png)
