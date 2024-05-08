﻿using System.Windows.Controls;

namespace ImageResizer.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
