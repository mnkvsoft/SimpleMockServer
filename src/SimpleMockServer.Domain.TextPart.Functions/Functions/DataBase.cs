﻿using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions;

abstract class DataBase : IWithStringArgumentFunction
{
    private readonly IDataProvider _dataProvider;

    protected DataBase(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    private DataName _name;
    private DataName? _rangeName;

    void IWithStringArgumentFunction.SetArgument(string argument)
    {
        if (!argument.Contains("."))
        {
            _name = new DataName(argument);
        }
        else
        {
            var splitted = argument.Split(".");

            if (splitted.Length > 2)
                throw new ArgumentException($"Invalid data name: '{argument}'");

            _name = new DataName(splitted[0]);
            _rangeName = new DataName(splitted[1]);
        }
    }

    protected DataRange GetRange()
    {
        var data = _dataProvider.GetData(_name);

        var range = _rangeName == null ? data.GetDefault() : data.Get(_rangeName.Value);
        return range;
    }
}
