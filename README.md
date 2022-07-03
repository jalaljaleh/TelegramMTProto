<div id="top"></div>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="#">
   
  </a>

  <h1 align="center">Telegram MTProto</h1>

  <p align="center">
    A .net project for Telegram MTProto
    <br />
    <a href="https://github.com/ShecanIr/TelegramMTProto/issues">Report Bug</a>
    ·
    <a href="https://github.com/ShecanIr/TelegramMTProto/issues">Request Feature</a>
  </p>
</div>

<div align="center">
  
</div>




<!-- ABOUT THE PROJECT -->
## About The Project
This is a project to create MTProto Proxy for Telegram

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- Use THE PROJECT -->
## How to use

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramMTProto;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var secret = GenerateRandomSecret();
            var mtprotoProxy = new TelegramMTProtoServer(secret, 1080);
            mtprotoProxy.Start();

            Console.WriteLine("press any key to exit...");
            Console.ReadLine();
        }
        public static string GenerateRandomSecret()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}

```

<p align="right">(<a href="#top">back to top</a>)</p>
        
        
        
<!-- LICENSE -->
## License

MIT License

Copyright (c) 2018 Jalal Jaleh

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>




<!-- CONTACT -->
## Contact

Jalal Jaleh - jalaljaleh@gmail.com

Project Link: [https://github.com/jalaljaleh/TelegramMTProto](https://github.com/jalaljaleh/TelegramMTProto)

<p align="right">(<a href="#top">back to top</a>)</p
