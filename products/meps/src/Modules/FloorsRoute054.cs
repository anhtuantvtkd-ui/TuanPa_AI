using System;using System.Linq;
namespace Tvxd.Meps {
 public sealed partial class Floors054 {
  public decimal? RouteHeight(string floorId,string roomId){var floor=floors.Single(f=>f.id==floorId);if(roomId==null)return floor.routeHeight;return floor.rooms.Single(r=>r.id==roomId).routeHeight??floor.routeHeight;}
  public void SetRouteHeight(string floorId,string roomId,decimal? value){if(value<0)throw new InvalidOperationException("CAO ĐỘ TUYẾN NGANG PHẢI KHÔNG ÂM.");var floor=floors.Single(f=>f.id==floorId);if(roomId==null)floor.routeHeight=value;else floor.rooms.Single(r=>r.id==roomId).routeHeight=value;}
 }
}
