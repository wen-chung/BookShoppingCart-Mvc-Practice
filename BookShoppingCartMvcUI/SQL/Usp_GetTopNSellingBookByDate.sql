CREATE PROCEDURE [dbo].[Usp_GetTopNSellingBookByDate]
@startDate DATETIME,@endDate DATETIME
AS

BEGIN

set NOCOUNT on;

WITH UnitSold as(
select od.BookId,SUM(od.Quantity) as TotalUnitSold
from [Order] o join OrderDetail od on o.Id =od.OrderId
where o.IsPaid=1 and o.IsDeleted = 0 and o.CreateDate BETWEEN @startDate and @endDate
GROUP BY od.BookId
) 

select top 5 b.BookName,b.AuthorName ,b.[Image],us.TotalUnitSold
from UnitSold us join book b on us.BookId = b.id
ORDER BY us.TotalUnitSold desc

END