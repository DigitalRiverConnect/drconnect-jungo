-- fix content item property names
update n2detail set Name = 'ApplicationTileColor' where Name = 'MsApplicationTileColor'
 and itemid in (select id from n2item where type = 'StartPage')
update n2detail set Name = 'ApplicationSquare70X70Logo' where Name = 'MsApplicationSquare70x70Logo'
 and itemid in (select id from n2item where type = 'StartPage')
update n2detail set Name = 'ApplicationSquare150X150Logo' where Name = 'MsApplicationSquare150x150Logo'
 and itemid in (select id from n2item where type = 'StartPage')
update n2detail set Name = 'ApplicationWide310X150Logo' where Name = 'MsApplicationWide310x150Logo'
 and itemid in (select id from n2item where type = 'StartPage')

update n2Detail set StringValue = 'MyStoreFooter' where StringValue = 'MsStoreFooter'
update n2Detail set StringValue = 'MyStoreSkinnyHeader' where StringValue = 'MsStoreSkinnyHeader'
update n2Detail set StringValue = 'MyStoreHeaderCheckout' where StringValue = 'MsStoreHeaderCheckout'
update n2Detail set StringValue = 'MyStoreNav' where StringValue = 'MsStoreNav'

-- MAKE A CHOICE
-- do either this:
delete from n2detail where itemid in (select id from n2item where type = 'livechatpart')
delete from n2item where type = 'livechatpart'
-- or this:
update n2item set type = 'AnalyticsPart' where type = 'LiveChatPart'

-- Get rid of LogOnPart
delete from n2detail where itemid in (select id from n2Item where type = 'LogOnPart')
delete from n2Item where type = 'LogOnPart'

-- Get rid of UpdateOrderPage
delete from n2detail where ItemID in (select id from n2Item where type = 'UpdateOrderPage')
delete from n2Item where type = 'UpdateOrderPage'

-- Get rid of ShopperAccountPage
delete from n2Detail where ItemID in (select ID from n2Item where ParentID in (select ID from n2item where type = 'ShopperAccountPage'))
delete from n2Item where ParentID in (select ID from n2item where type = 'ShopperAccountPage')
delete from n2Detail where ItemID in (select ID from n2item where type = 'ShopperAccountPage')
delete from n2item where type = 'ShopperAccountPage'

-- Get rid of CheckoutPage
delete from n2Detail where ItemID in (select ID from n2Item where ParentID in (select ID from n2item where type = 'CheckoutPage'))
delete from n2Item where ParentID in (select ID from n2item where type = 'CheckoutPage')
delete from n2Detail where ItemID in (select ID from n2item where type = 'CheckoutPage')
delete from n2item where type = 'CheckoutPage'

-- more
delete from n2Detail where ItemID in (select ID from n2Item where ParentID in (select ID from n2item where type = 'ProductAnswerDeskDetailsPart'))
delete from n2Item where ParentID in (select ID from n2item where type = 'ProductAnswerDeskDetailsPart')
delete from n2detail where itemid in (select id from n2Item where type = 'ProductAnswerDeskDetailsPart')
delete from n2item where type = 'ProductAnswerDeskDetailsPart'
