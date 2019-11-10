using System.Collections;
using System.Collections.Generic;

public interface IData {
    void formattingData(ref Dictionary<string, Data> datas, ref float[][] writeData, ref float[,] formattedData);
}
