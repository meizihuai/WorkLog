Module Module1


    Structure ProAndCity
        Dim province As String
        Dim cityList As List(Of cityInfo)
        Sub New(ByVal _province As String)
            province = _province
            cityList = New List(Of cityInfo)
        End Sub
        Sub New(ByVal _province As String, _cityName As String, _districtName As String)
            province = _province
            cityList = New List(Of cityInfo)
            cityList.Add(New cityInfo(_cityName, _districtName))
        End Sub
    End Structure
    Structure cityInfo
        Dim city As String
        Dim district As List(Of String)
        Sub New(ByVal _city As String)
            Me.city = _city
            district = New List(Of String)
        End Sub
        Sub New(ByVal _city As String, _districtName As String)
            Me.city = _city
            district = New List(Of String)
            district.Add(_districtName)
        End Sub
    End Structure
End Module
