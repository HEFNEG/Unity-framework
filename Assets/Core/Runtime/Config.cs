using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Config {
    public static readonly string assetPath = Application.streamingAssetsPath + "/";
    public const int bundleLifeTime = 10;

    public const string pkgFile = "pkg.json";
    public const string bundleExtend = ".bundle";

    // UIEvent Name
    public const string UI_CLICK_EVENT = "Click";
    public const string UI_POINT_ENTER_EVENT = "PointEnter";
    public const string UI_POINT_EXIT_EVENT = "PointExit";
    public const string UI_POINT_DOWN_EVENT = "PointDown";
    public const string UI_POINT_UP_EVENT = "PointUp";
    public const string UI_SCROLL_EVENT = "Scroll";
    public const string UI_BEGIN_DRAG_EVENT = "BeginDrag";
    public const string UI_END_DRAG_EVENT = "EndDrag";
    public const string UI_DRAG_EVENT = "Drag";
    public const string UI_DROP_EVENT = "DROP";
}
