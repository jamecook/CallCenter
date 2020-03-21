------------------------------------- ������ �� ��������
SELECT r.id "�",case when r.is_immediate= 1 then '���������' else '�������' end "���������",s.name "��",r.create_time "�������",tp.name "������",t.name "�������",rs.Description,

count(ch.UniqueId) "������������",
sum(case when ch.Direction in ('in','pickup') then 1 else 0 end) "��������", sum(case when ch.Direction in ('out','callback') then 1 else 0 end) "���������",

sum(case when ch.Direction in ('out','callback') and ch.BridgedTime is not null then 1 else 0 end) "�����������������",
sum(case when ch.Direction in ('in','pickup') then TIMESTAMPDIFF(SECOND,ch.BridgedTime,ch.EndTime) else 0 end) "�����������",
sum(case when ch.Direction in ('out','callback') then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "������������"
 FROM Requests r
join ServiceCompanies s on s.id = r.service_company_id
join RequestTypes t on t.id = r.type_id
join RequestTypes tp on tp.id = t.parrent_id
join RequestState rs on rs.id = r.state_id
left join RequestCalls rc on rc.request_id = r.id
left join asterisk.ChannelHistory ch on ch.UniqueId = rc.uniqueID
where create_time > 20190901
 group by r.id;
 
 
SELECT r.id "�",case when r.is_immediate= 1 then '���������' else '�������' end "���������",s.name "��",r.create_time "�������",tp.name "������",t.name "�������",rs.Description,

count(ch.UniqueId) "������������",
sum(case when ch.Direction in ('in','pickup') then TIMESTAMPDIFF(SECOND,ch.BridgedTime,ch.EndTime)
  when ch.Direction in ('out','callback') then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "��������",
sum(case when ch.Direction in ('in','pickup') and w.id is not null then TIMESTAMPDIFF(SECOND,ch.BridgedTime,ch.EndTime)
  when ch.Direction in ('out','callback') and w.id is not null then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "��������_�_��",
  
sum(case when ch.Direction in ('in','pickup') then 1 else 0 end) "��������", sum(case when ch.Direction in ('out','callback') then 1 else 0 end) "���������",
sum(case when ch.Direction in ('out','callback') and ch.BridgedTime is not null then 1 else 0 end) "�����������������",
sum(case when ch.Direction in ('in','pickup') then TIMESTAMPDIFF(SECOND,ch.BridgedTime,ch.EndTime) else 0 end) "�����������",
sum(case when ch.Direction in ('out','callback') then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "������������",

sum(case when ch.Direction in ('in','pickup') and w.id is not null then 1 else 0 end) "��������_�_��",
sum(case when ch.Direction in ('out','callback') and w.id is not null then 1 else 0 end) "���������_�_��",
sum(case when ch.Direction in ('out','callback') and ch.BridgedTime is not null and w.id is not null then 1 else 0 end) "�����������������_�_��",
sum(case when ch.Direction in ('in','pickup') and w.id is not null then TIMESTAMPDIFF(SECOND,ch.BridgedTime,ch.EndTime) else 0 end) "�����������_�_��",
sum(case when ch.Direction in ('out','callback') and w.id is not null then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "������������_�_��"


 FROM Requests r
join ServiceCompanies s on s.id = r.service_company_id
join RequestTypes t on t.id = r.type_id
join RequestTypes tp on tp.id = t.parrent_id
join RequestState rs on rs.id = r.state_id
left join RequestCalls rc on rc.request_id = r.id
left join asterisk.ChannelHistory ch on ch.UniqueId = rc.uniqueID
left join CallCenter.Workers w on w.phone = ch.PhoneNum and not exists (select 1 from CallCenter.Workers w2 where w2.phone = w.phone and w2.id> w.id)

where create_time > 20190101
 group by r.id;
 
 
---------------------------- ����� �� �������
 
 SELECT DATE_FORMAT(ch.CreateTime,'%Y.%m.%d') "����",
sum(case when ch.Direction in ('in','pickup','out','callback') then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "��������",
sum(case when ch.Direction in ('in','pickup','out','callback') then TIMESTAMPDIFF(SECOND,ch.BridgedTime,ch.EndTime) else 0 end) "������������������",
sum(case when ch.Direction in ('in','pickup') then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "�����������",
sum(case when ch.Direction in ('out','callback') then TIMESTAMPDIFF(SECOND,ch.CreateTime,ch.EndTime) else 0 end) "������������"
 FROM asterisk.ChannelHistory ch
where ch.ServiceComp = 'yit_service'and
ch.UniqueId >= '1552128123.322928' and ch.UniqueId = ch.LinkedId and ch.Direction is not null
group by DATE_FORMAT(ch.CreateTime,'%Y.%m.%d');


---------------------------- �������� �������
select CallDirection "�����������",CallerIDNum "�����",CreateTime "�����_������", TalkTime "�����������������_���������",WaitingTime "�����_��������",
ClearWaitingTime "������_�����_��������",IVRTime "�����������������_IVR",
CallTime "�����_�����_������",
concat(u.SurName,' ',if(u.FirstName is null,'',u.FirstName),' ',if(u.PatrName is null,'',u.PatrName)) "���������",
case when u.id not in (106,107,108,109,110,111) then 'Dispex' else  '����' end "��_����������",
RequestId "������", ServiceCompanyName from
(
select C.UniqueID AS UniqueId, C.Direction AS CallDirection,
(case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) AS CallerIDNum,
C.CreateTime AS CreateTime,
C.AnswerTime AS AnswerTime,
C.EndTime AS EndTime,
C.BridgedTime AS BridgedTime,
C.MonitorFile AS MonitorFile,
timestampdiff(SECOND, C.BridgedTime, C.EndTime) AS TalkTime,
timestampdiff(SECOND, C.CreateTime, C.EndTime) as CallTime,
  (timestampdiff(SECOND, C.CreateTime, C.EndTime) - ifnull(timestampdiff(SECOND, C.BridgedTime, C.EndTime), 0)) AS WaitingTime,
timestampdiff(SECOND, C.queue_time, ifnull(C.BridgedTime, C.EndTime)) AS ClearWaitingTime,
timestampdiff(SECOND, C.CreateTime, C.queue_time) AS IVRTime,
ifnull(C.UserId, max(C2.UserId)) userId,
(select group_concat(r.request_id order by r.request_id separator ', ') from CallCenter.RequestCalls r where r.uniqueID = C.UniqueID) AS RequestId,
sc.Name ServiceCompanyName,
group_concat(concat(C2.peer_number, ':', C2.ChannelState) order by C2.UniqueId desc separator ',') as redirect_phone,
C.ivr_menu,C.ivr_dial
FROM asterisk.ChannelHistory C
left join asterisk.ChannelBridges B on B.UniqueId = C.UniqueId
left join asterisk.ChannelHistory C2 on C2.BridgeId = B.BridgeId and C2.UniqueId <> C.UniqueId
left join CallCenter.ServiceCompanies sc on sc.trunk_name = C.ServiceComp
left join CallCenter.RequestCalls r on r.uniqueID = C.UniqueID
where C.UniqueId >= '1552128123.322928' and C.UniqueId = C.LinkedId and C.Direction is not null and C.CreateTime > 20190601 -- and C.ServiceComp = 'yit_service'
and C.Context not in ('autoring','ringupcalls')
group by C.UniqueId
) a
left join CallCenter.Users u on u.id = a.userId

------------------���������� ������� �������� ������ ����
select CallDirection "�����������",CallerIDNum "�����",CreateTime "�����_������", TalkTime "�����������������_���������",WaitingTime "�����_��������",
ClearWaitingTime "������_�����_��������",IVRTime "�����������������_IVR",
CallTime "�����_�����_������",
concat(u.SurName,' ',if(u.FirstName is null,'',u.FirstName),' ',if(u.PatrName is null,'',u.PatrName)) "���������",
case when u.id not in (106,107,108,109,110,111) then 'Dispex' else  '����' end "��_����������",
RequestId "������", ServiceCompanyName from
(
select C.UniqueID AS UniqueId, C.Direction AS CallDirection,
(case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) AS CallerIDNum,
C.CreateTime AS CreateTime,
C.AnswerTime AS AnswerTime,
C.EndTime AS EndTime,
C.BridgedTime AS BridgedTime,
C.MonitorFile AS MonitorFile,
timestampdiff(SECOND, C.BridgedTime, C.EndTime) AS TalkTime,
timestampdiff(SECOND, C.CreateTime, C.EndTime) as CallTime,
  (timestampdiff(SECOND, C.CreateTime, C.EndTime) - ifnull(timestampdiff(SECOND, C.BridgedTime, C.EndTime), 0)) AS WaitingTime,
timestampdiff(SECOND, C.queue_time, ifnull(C.BridgedTime, C.EndTime)) AS ClearWaitingTime,
timestampdiff(SECOND, C.CreateTime, C.queue_time) AS IVRTime,
ifnull(C.UserId, max(C2.UserId)) userId,
(select group_concat(r.request_id order by r.request_id separator ', ') from CallCenter.RequestCalls r where r.uniqueID = C.UniqueID) AS RequestId,
sc.Name ServiceCompanyName,
group_concat(concat(C2.peer_number, ':', C2.ChannelState) order by C2.UniqueId desc separator ',') as redirect_phone,
C.ivr_menu,C.ivr_dial
FROM asterisk.ChannelHistory C
left join asterisk.ChannelBridges B on B.UniqueId = C.UniqueId
left join asterisk.ChannelHistory C2 on C2.BridgeId = B.BridgeId and C2.UniqueId <> C.UniqueId
left join CallCenter.ServiceCompanies sc on sc.trunk_name = C.ServiceComp
left join CallCenter.RequestCalls r on r.uniqueID = C.UniqueID
where C.UniqueId >= '1552128123.322928' and C.UniqueId = C.LinkedId and C.Direction is not null and C.CreateTime > 20200301 and C.ServiceComp in ('yit_service','amega')
and C.Context not in ('autoring','ringupcalls')
group by C.UniqueId
) a
left join CallCenter.Users u on u.id = a.userId
where a.WaitingTime > 80 and u.id not in (106,107,108,109,110,111) and CallDirection = 'in'


-------------------------------���������� �� �����������
create table StatDispatcher as

select CallDirection ,CallerIDNum ,CreateTime , TalkTime ,WaitingTime ,CallTime ,
u.id UserId, concat(u.SurName,' ',if(u.FirstName is null,'',u.FirstName),' ',if(u.PatrName is null,'',u.PatrName)) UserFio,
RequestId, ServiceCompanyName from
(
select C.UniqueID AS UniqueId, C.Direction AS CallDirection,
(case when C.PhoneNum is not null then C.PhoneNum when(C.CallerIDNum in ('scvip500415','594555')) then C.Exten else C.CallerIDNum end) AS CallerIDNum,
C.CreateTime AS CreateTime,
C.AnswerTime AS AnswerTime,
C.EndTime AS EndTime,
C.BridgedTime AS BridgedTime,
C.MonitorFile AS MonitorFile,
timestampdiff(SECOND, C.BridgedTime, C.EndTime) AS TalkTime,
timestampdiff(SECOND, C.CreateTime, C.EndTime) as CallTime,
  (timestampdiff(SECOND, C.CreateTime, C.EndTime) - ifnull(timestampdiff(SECOND, C.BridgedTime, C.EndTime), 0)) AS WaitingTime,
ifnull(C.UserId, max(C2.UserId)) userId,
(select group_concat(r.request_id order by r.request_id separator ', ') from CallCenter.RequestCalls r where r.uniqueID = C.UniqueID) AS RequestId,
sc.Name ServiceCompanyName,
group_concat(concat(C2.peer_number, ':', C2.ChannelState) order by C2.UniqueId desc separator ',') as redirect_phone,
C.ivr_menu,C.ivr_dial
FROM asterisk.ChannelHistory C
left join asterisk.ChannelBridges B on B.UniqueId = C.UniqueId
left join asterisk.ChannelHistory C2 on C2.BridgeId = B.BridgeId and C2.UniqueId <> C.UniqueId
left join CallCenter.ServiceCompanies sc on sc.trunk_name = C.ServiceComp
left join CallCenter.RequestCalls r on r.uniqueID = C.UniqueID
where C.UniqueId >= '1552128123.322928' and C.UniqueId = C.LinkedId and C.Direction is not null -- and C.ServiceComp = 'yit_service'
and C.Context not in ('autoring','ringupcalls')
group by C.UniqueId
) a
left join CallCenter.Users u on u.id = a.userId;



---------------------------------������ ����������� � ����� � ��������
select s.name '�����',concat(h.building,if(h.corps is null,'',concat('/',h.corps))) '���',
t.name '������',concat(w.sur_name,if(w.first_name is null,'',concat(' ',w.first_name)),if(w.patr_name is null,'',concat(' ',w.patr_name))) '�����������'
-- select *
from Houses h
join Streets s on s.id = h.street_id
join WorkerHouseAndType wt on wt.house_id = h.id
join Workers w on w.id = wt.worker_id
 left join RequestTypes t on t.id = wt.type_id
where h.service_company_id = 39;

----------------------�����--------------------
SELECT -- is_client,
YEAR(create_date), MONTH(create_date),sum(price*sms_count) FROM SMSRequest S
group by YEAR(create_date), MONTH(create_date)
order by YEAR(create_date), MONTH(create_date) -- ,is_client

-----------------����� ����
SELECT c.id,c.name,sum(sms_count),sum(sms_count*price) FROM CallCenter.SMSRequest s
join CallCenter.Requests r on r.id = s.request_id
join CallCenter.ServiceCompanies c on c.id = r.service_company_id
where s.create_date between 20200201 and 20200301 and c.id in (17,48,88,142)
group by c.id;

SELECT c.name "��",create_date "����",s.phone "�������",message "�����",sms_count "�����_���", sms_count*price "����" FROM CallCenter.SMSRequest s
join CallCenter.Requests r on r.id = s.request_id
join CallCenter.ServiceCompanies c on c.id = r.service_company_id
where s.create_date between 20200201 and 20200301 and c.id in (17,48,88,142)