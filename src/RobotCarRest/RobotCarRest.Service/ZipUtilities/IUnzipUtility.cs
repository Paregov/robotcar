// Copyright © Svetoslav Paregov. All rights reserved.

namespace NetworkController.ZipUtilities;

public interface IUnzipUtility
{
    public bool UnzipToCurrentFolder(string zipFilePath);
}
