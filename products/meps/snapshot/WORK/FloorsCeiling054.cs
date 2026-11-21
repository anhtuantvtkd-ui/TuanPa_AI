using System;using System.Linq;
namespace Tvxd.Meps {
 public sealed partial class Floors054 {
  void ValidateCeilings(){if(floors.Any(f=>f.ceilingHeight<0||f.rooms.Any(r=>r.ceilingHeight<0)))throw new InvalidOperationException("CAO ĐỘ TRẦN PHẢI KHÔNG ÂM.");}
  public decimal? CeilingHeight(string floorId,string roomId){var f=floors.Single(x=>x.id==floorId);return roomId==null?f.ceilingHeight:f.rooms.Single(x=>x.id==roomId).ceilingHeight??f.ceilingHeight;}
  public void SetCeilingHeight(string floorId,string roomId,decimal? value){if(value<0)throw new InvalidOperationException("CAO ĐỘ TRẦN PHẢI KHÔNG ÂM.");var f=floors.Single(x=>x.id==floorId);if(roomId==null)f.ceilingHeight=value;else f.rooms.Single(x=>x.id==roomId).ceilingHeight=value;}
 }
}
